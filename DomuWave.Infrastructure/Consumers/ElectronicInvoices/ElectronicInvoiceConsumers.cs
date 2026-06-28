using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Clients;
using DomuWave.Services.Command.ElectronicInvoices;
using DomuWave.Services.Dto.ElectronicInvoice;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SimpleMediator.Core;
using System.Diagnostics;

namespace DomuWave.Services.Consumers;

/// <summary>
/// Scarica le fatture passive dal provider SdI, deduplica su SdiIdentifier, prova il match
/// del fornitore per partita IVA e persiste le nuove fatture. Restituisce quelle importate.
/// </summary>
public class SyncEInvoicesCommandConsumer
    : InMemoryConsumerBase<SyncEInvoicesCommand, IList<ElectronicInvoiceReadDto>>
{
    private readonly IEInvoiceService _eInvoiceService;
    private readonly IUserService     _userService;
    private readonly ILogger<SyncEInvoicesCommandConsumer> _logger;

    public SyncEInvoicesCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IEInvoiceService        eInvoiceService,
        IUserService            userService,
        ILogger<SyncEInvoicesCommandConsumer> logger) : base(sessionFactoryProvider)
    {
        _eInvoiceService = eInvoiceService;
        _userService     = userService;
        _logger          = logger;
    }

    protected override async Task<IList<ElectronicInvoiceReadDto>> Consume(
        SyncEInvoicesCommand command,
        IMediationContext    mediationContext,
        CancellationToken    cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation(
            "Sync fatture elettroniche avviato per condominio {CondominiumId} dal {From:d} al {To:d} (utente {UserId}).",
            command.CondominiumId, command.From, command.To, command.CurrentUserId);

        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (command.To < command.From)
        {
            _logger.LogWarning(
                "Sync fatture: intervallo date non valido (dal {From:d} al {To:d}) per condominio {CondominiumId}.",
                command.From, command.To, command.CondominiumId);
            throw new ValidatorException("L'intervallo di date non è valido.");
        }

        var condominium = await session.Query<Models.Condominium>()
            .FirstOrDefaultAsync(c => c.Id == command.CondominiumId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Condominio non trovato.");

        // Scarica dal provider (le validazioni di configurazione stanno nel service).
        var downloaded = await _eInvoiceService
            .DownloadPassiveInvoicesAsync(condominium, command.From, command.To, cancellationToken)
            .ConfigureAwait(false);

        if (downloaded == null || downloaded.Count == 0)
        {
            _logger.LogInformation(
                "Sync fatture: nessuna fattura restituita dal provider per condominio {CondominiumId}. ({ElapsedMs} ms)",
                command.CondominiumId, sw.ElapsedMilliseconds);
            return new List<ElectronicInvoiceReadDto>();
        }

        // Identificativi SdI già presenti per il condominio (deduplica).
        var existingSdiIds = (await session.Query<ElectronicInvoice>()
                .Where(e => e.Condominium.Id == condominium.Id && !e.IsDeleted)
                .Select(e => e.SdiIdentifier)
                .ToListAsync(cancellationToken).ConfigureAwait(false))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var imported = new List<ElectronicInvoice>();

        foreach (var item in downloaded)
        {
            if (string.IsNullOrWhiteSpace(item.SdiIdentifier) || existingSdiIds.Contains(item.SdiIdentifier))
                continue;

            // Match fornitore per partita IVA (best-effort).
            Supplier supplier = null;
            if (!string.IsNullOrWhiteSpace(item.SupplierVat))
            {
                supplier = await session.Query<Supplier>()
                    .FirstOrDefaultAsync(s => s.VatNumber == item.SupplierVat
                                           && s.Tenant.Id == condominium.Tenant.Id
                                           && !s.IsDeleted, cancellationToken)
                    .ConfigureAwait(false);
            }

            var invoice = new ElectronicInvoice
            {
                Tenant          = condominium.Tenant,
                Condominium     = condominium,
                Supplier        = supplier,
                StatusId        = 0, // New
                SdiIdentifier   = item.SdiIdentifier,
                InvoiceNumber   = item.InvoiceNumber,
                InvoiceDate     = item.InvoiceDate,
                SupplierVat     = item.SupplierVat,
                SupplierTaxCode = item.SupplierTaxCode,
                SupplierName    = item.SupplierName,
                TotalAmount     = item.TotalAmount,
                XmlContent      = item.XmlContent,
            };
            invoice.Trace(currentUser);
            await session.SaveAsync(invoice, cancellationToken).ConfigureAwait(false);

            existingSdiIds.Add(item.SdiIdentifier);
            imported.Add(invoice);
        }

        condominium.EInvoiceLastSyncDate = DateTime.UtcNow;
        condominium.Trace(currentUser);
        await session.UpdateAsync(condominium, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        var skipped = downloaded.Count - imported.Count;
        _logger.LogInformation(
            "Sync fatture completato per condominio {CondominiumId}: {Imported} importate, {Skipped} ignorate (duplicate/non valide) su {Total} ricevute. ({ElapsedMs} ms)",
            command.CondominiumId, imported.Count, skipped, downloaded.Count, sw.ElapsedMilliseconds);

        return imported.Select(i => i.ToReadDto()).ToList();
    }
}

/// <summary>
/// Nomi dei provider SdI, allineati a EInvoiceProviderLookup (lato SQL).
/// </summary>
internal static class EInvoiceProviderNames
{
    public static string Of(int? id) => id switch
    {
        1 => "Acube",
        2 => "Aruba",
        3 => "FattureInCloud",
        _ => "None",
    };
}

/// <summary>
/// Costruisce il DTO di configurazione, risolvendo la P.IVA effettiva
/// (override fatture se presente, altrimenti quella dell'anagrafica condominio).
/// </summary>
internal static class EInvoiceConfigMapper
{
    public static EInvoiceConfigReadDto ToReadDto(Models.Condominium c)
    {
        var effective = !string.IsNullOrWhiteSpace(c.EInvoiceVatNumber)
            ? c.EInvoiceVatNumber
            : c.VatNumber;

        return new EInvoiceConfigReadDto
        {
            CondominiumId        = c.Id,
            ProviderId           = c.EInvoiceProviderId,
            ProviderName         = EInvoiceProviderNames.Of(c.EInvoiceProviderId),
            VatNumberOverride    = c.EInvoiceVatNumber,
            CondominiumVatNumber = c.VatNumber,
            EffectiveVatNumber   = effective,
            HasApiKey            = !string.IsNullOrWhiteSpace(c.EInvoiceApiKey),
            LastSyncDate         = c.EInvoiceLastSyncDate,
        };
    }
}

/// <summary>
/// Restituisce la configurazione corrente del download fatture per un condominio.
/// Non espone mai la chiave API (solo il flag di presenza).
/// </summary>
public class GetEInvoiceConfigCommandConsumer
    : InMemoryConsumerBase<GetEInvoiceConfigCommand, EInvoiceConfigReadDto>
{
    public GetEInvoiceConfigCommandConsumer(ISessionFactoryProvider sessionFactoryProvider)
        : base(sessionFactoryProvider) { }

    protected override async Task<EInvoiceConfigReadDto> Consume(
        GetEInvoiceConfigCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var condominium = await session.Query<Models.Condominium>()
            .FirstOrDefaultAsync(c => c.Id == command.CondominiumId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Condominio non trovato.");

        return EInvoiceConfigMapper.ToReadDto(condominium);
    }
}

/// <summary>
/// Salva la configurazione del download fatture per un condominio. La chiave API viene
/// cifrata e aggiornata solo se valorizzata (altrimenti resta invariata).
/// </summary>
public class UpdateEInvoiceConfigCommandConsumer
    : InMemoryConsumerBase<UpdateEInvoiceConfigCommand, EInvoiceConfigReadDto>
{
    private readonly IUserService _userService;
    private readonly IEInvoiceSecretProtector _secretProtector;
    private readonly ILogger<UpdateEInvoiceConfigCommandConsumer> _logger;

    public UpdateEInvoiceConfigCommandConsumer(
        ISessionFactoryProvider  sessionFactoryProvider,
        IUserService             userService,
        IEInvoiceSecretProtector secretProtector,
        ILogger<UpdateEInvoiceConfigCommandConsumer> logger) : base(sessionFactoryProvider)
    {
        _userService     = userService;
        _secretProtector = secretProtector;
        _logger          = logger;
    }

    protected override async Task<EInvoiceConfigReadDto> Consume(
        UpdateEInvoiceConfigCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var condominium = await session.Query<Models.Condominium>()
            .FirstOrDefaultAsync(c => c.Id == command.CondominiumId && !c.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Condominio non trovato.");

        var dto = command.Dto ?? new EInvoiceConfigUpdateDto();

        condominium.EInvoiceProviderId = dto.ProviderId;
        // Override opzionale: se vuoto, si userà la P.IVA dell'anagrafica condominio.
        condominium.EInvoiceVatNumber  = string.IsNullOrWhiteSpace(dto.VatNumberOverride)
            ? null
            : dto.VatNumberOverride.Trim();

        // La chiave si aggiorna solo se inviata: così l'admin non deve re-inserirla a ogni salvataggio.
        var apiKeyUpdated = !string.IsNullOrWhiteSpace(dto.ApiKey);
        if (apiKeyUpdated)
            condominium.EInvoiceApiKey = _secretProtector.Protect(dto.ApiKey.Trim());

        condominium.Trace(currentUser);
        await session.UpdateAsync(condominium, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        // NB: la chiave API non viene mai loggata, solo il flag di aggiornamento.
        _logger.LogInformation(
            "Configurazione fatture elettroniche aggiornata per condominio {CondominiumId} (utente {UserId}): provider {ProviderId}, override P.IVA {HasVatOverride}, chiave API aggiornata {ApiKeyUpdated}.",
            command.CondominiumId, command.CurrentUserId, condominium.EInvoiceProviderId,
            !string.IsNullOrWhiteSpace(condominium.EInvoiceVatNumber), apiKeyUpdated);

        return EInvoiceConfigMapper.ToReadDto(condominium);
    }
}

/// <summary>
/// Restituisce le fatture elettroniche scaricate per un condominio.
/// </summary>
public class GetEInvoicesByCondominiumCommandConsumer
    : InMemoryConsumerBase<GetEInvoicesByCondominiumCommand, IList<ElectronicInvoiceReadDto>>
{
    public GetEInvoicesByCondominiumCommandConsumer(ISessionFactoryProvider sessionFactoryProvider)
        : base(sessionFactoryProvider) { }

    protected override async Task<IList<ElectronicInvoiceReadDto>> Consume(
        GetEInvoicesByCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var invoices = await session.Query<ElectronicInvoice>()
            .Where(e => e.Condominium.Id == command.CondominiumId && !e.IsDeleted)
            .OrderByDescending(e => e.InvoiceDate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return invoices.Select(i => i.ToReadDto()).ToList();
    }
}

/// <summary>
/// Collega una fattura scaricata a una spesa esistente, portandola allo stato Linked.
/// </summary>
public class LinkEInvoiceToExpenseCommandConsumer
    : InMemoryConsumerBase<LinkEInvoiceToExpenseCommand, ElectronicInvoiceReadDto>
{
    private readonly IUserService _userService;
    private readonly ILogger<LinkEInvoiceToExpenseCommandConsumer> _logger;

    public LinkEInvoiceToExpenseCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService,
        ILogger<LinkEInvoiceToExpenseCommandConsumer> logger) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _logger      = logger;
    }

    protected override async Task<ElectronicInvoiceReadDto> Consume(
        LinkEInvoiceToExpenseCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var invoice = await session.Query<ElectronicInvoice>()
            .FirstOrDefaultAsync(e => e.Id == command.InvoiceId && !e.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Fattura elettronica non trovata.");

        var expense = await session.Query<Expense>()
            .FirstOrDefaultAsync(e => e.Id == command.ExpenseId && !e.IsDeleted, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Spesa non trovata.");

        invoice.Expense  = expense;
        invoice.StatusId = 1; // Linked
        invoice.Trace(currentUser);
        await session.UpdateAsync(invoice, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Fattura elettronica {InvoiceId} collegata alla spesa {ExpenseId} (utente {UserId}).",
            command.InvoiceId, command.ExpenseId, command.CurrentUserId);

        return invoice.ToReadDto();
    }
}
