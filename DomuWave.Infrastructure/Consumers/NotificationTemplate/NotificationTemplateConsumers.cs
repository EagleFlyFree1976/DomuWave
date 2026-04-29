using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.NotificationTemplate;
using DomuWave.Services.Dto.NotificationTemplate;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

// ── GET BY CONDOMINIUM ────────────────────────────────────────────────────────

public class GetNotificationTemplatesByCondominiumCommandConsumer
    : InMemoryConsumerBase<GetNotificationTemplatesByCondominiumCommand, IList<NotificationTemplateReadDto>>
{
    private readonly IUserService _userService;

    public GetNotificationTemplatesByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<NotificationTemplateReadDto>> Consume(
        GetNotificationTemplatesByCondominiumCommand command,
        IMediationContext                             mediationContext,
        CancellationToken                            cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var list = await session.Query<NotificationTemplate>()
            .Where(t => t.Condominium.Id == command.CondominiumId && !t.IsDeleted)
            .OrderBy(t => t.CommunicationType)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return list.Select(t => t.ToReadDto()).ToList();
    }
}

// ── GET BY ID ─────────────────────────────────────────────────────────────────

public class GetNotificationTemplateByIdCommandConsumer
    : InMemoryConsumerBase<GetNotificationTemplateByIdCommand, NotificationTemplateReadDto?>
{
    private readonly IUserService _userService;

    public GetNotificationTemplateByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<NotificationTemplateReadDto?> Consume(
        GetNotificationTemplateByIdCommand command,
        IMediationContext                   mediationContext,
        CancellationToken                  cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<NotificationTemplate>()
            .Where(t => t.Id == command.TemplateId && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return entity?.ToReadDto();
    }
}

// ── CREATE ────────────────────────────────────────────────────────────────────

public class CreateNotificationTemplateCommandConsumer
    : InMemoryConsumerBase<CreateNotificationTemplateCommand, NotificationTemplateReadDto>
{
    private readonly IUserService _userService;

    public CreateNotificationTemplateCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<NotificationTemplateReadDto> Consume(
        CreateNotificationTemplateCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                 cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var condominium = await session.GetAsync<Models.Condominium>(command.Dto.CondominiumId, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Condominio non trovato.");

        if (string.IsNullOrWhiteSpace(command.Dto.Name))
            throw new ValidatorException("Il nome del template è obbligatorio.");
        if (string.IsNullOrWhiteSpace(command.Dto.SubjectTemplate))
            throw new ValidatorException("L'oggetto del template è obbligatorio.");
        if (string.IsNullOrWhiteSpace(command.Dto.BodyTemplate))
            throw new ValidatorException("Il corpo del template è obbligatorio.");

        // Se IsDefault, rimuovi il flag dagli altri template dello stesso tipo
        if (command.Dto.IsDefault)
            await ClearDefaultForType(command.Dto.CondominiumId, command.Dto.CommunicationType, null, cancellationToken);

        var entity = command.Dto.ToEntity(condominium, condominium.Tenant);
        entity.Trace(currentUser);

        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return entity.ToReadDto();
    }

    private async Task ClearDefaultForType(int condominiumId, string type, int? exceptId, CancellationToken ct)
    {
        var existing = await session.Query<NotificationTemplate>()
            .Where(t => t.Condominium.Id == condominiumId && t.CommunicationType == type
                     && t.IsDefault && !t.IsDeleted && (exceptId == null || t.Id != exceptId))
            .ToListAsync(ct).ConfigureAwait(false);

        foreach (var t in existing)
        {
            t.IsDefault = false;
            await session.UpdateAsync(t, ct).ConfigureAwait(false);
        }
    }
}

// ── UPDATE ────────────────────────────────────────────────────────────────────

public class UpdateNotificationTemplateCommandConsumer
    : InMemoryConsumerBase<UpdateNotificationTemplateCommand, NotificationTemplateReadDto?>
{
    private readonly IUserService _userService;

    public UpdateNotificationTemplateCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<NotificationTemplateReadDto?> Consume(
        UpdateNotificationTemplateCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                 cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<NotificationTemplate>()
            .Where(t => t.Id == command.Id && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (entity == null) return null;

        if (command.Dto.IsDefault == true)
        {
            var others = await session.Query<NotificationTemplate>()
                .Where(t => t.Condominium.Id == entity.Condominium.Id
                         && t.CommunicationType == entity.CommunicationType
                         && t.IsDefault && !t.IsDeleted && t.Id != entity.Id)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            foreach (var t in others) { t.IsDefault = false; await session.UpdateAsync(t, cancellationToken).ConfigureAwait(false); }
        }

        entity.ApplyUpdate(command.Dto);
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}

// ── DELETE ────────────────────────────────────────────────────────────────────

public class DeleteNotificationTemplateCommandConsumer
    : InMemoryConsumerBase<DeleteNotificationTemplateCommand, bool>
{
    private readonly IUserService _userService;

    public DeleteNotificationTemplateCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<bool> Consume(
        DeleteNotificationTemplateCommand command,
        IMediationContext                  mediationContext,
        CancellationToken                 cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.Query<NotificationTemplate>()
            .Where(t => t.Id == command.TemplateId && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}

// ── SEED DEFAULT TEMPLATES ────────────────────────────────────────────────────

public class SeedDefaultNotificationTemplatesCommandConsumer
    : InMemoryConsumerBase<SeedDefaultNotificationTemplatesCommand, IList<NotificationTemplateReadDto>>
{
    private readonly IUserService _userService;

    public SeedDefaultNotificationTemplatesCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<NotificationTemplateReadDto>> Consume(
        SeedDefaultNotificationTemplatesCommand command,
        IMediationContext                        mediationContext,
        CancellationToken                       cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var condominium = await session.GetAsync<Models.Condominium>(command.CondominiumId, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Condominio non trovato.");

        var defaults = new[]
        {
            new { Type = "FeeNotice",    Name = "Avviso di pagamento quote",
                  Subject = "Avviso di pagamento quote condominiali — {{CondominiumName}}",
                  Body =
                      "Gentile {{RecipientName}},\n\n" +
                      "Le ricordiamo che risultano le seguenti quote condominiali a Suo carico relative all'unità {{UnitNumber}}:\n\n" +
                      "{{FeeTable}}\n\n" +
                      "Totale da versare: {{TotalAmount}}\n\n" +
                      "Il pagamento dovrà essere effettuato tramite bonifico bancario al seguente IBAN:\n" +
                      "{{Iban}}\n" +
                      "causale: Quote condominiali — {{CondominiumName}} — {{FiscalYearCode}}\n\n" +
                      "Per qualsiasi chiarimento non esiti a contattarci.\n\n" +
                      "Cordiali saluti,\n{{AdministratorName}}\n{{AdministratorEmail}}\n{{AdministratorPhone}}" },
            new { Type = "Notice",       Name = "Avviso generico",
                  Subject = "{{CommunicationTitle}} — {{CondominiumName}}",
                  Body =
                      "Gentile {{RecipientName}},\n\n" +
                      "{{CommunicationBody}}\n\n" +
                      "Cordiali saluti,\n{{AdministratorName}}\n{{AdministratorEmail}}\n{{AdministratorPhone}}" },
            new { Type = "Meeting",      Name = "Convocazione assemblea",
                  Subject = "Convocazione assemblea condominiale — {{CondominiumName}}",
                  Body =
                      "Gentile {{RecipientName}},\n\n" +
                      "La informiamo che è stata convocata un'assemblea condominiale.\n\n" +
                      "{{CommunicationBody}}\n\n" +
                      "La Sua presenza è gradita.\n\n" +
                      "Cordiali saluti,\n{{AdministratorName}}\n{{AdministratorEmail}}\n{{AdministratorPhone}}" },
            new { Type = "Maintenance",  Name = "Comunicazione lavori",
                  Subject = "Comunicazione lavori — {{CondominiumName}}",
                  Body =
                      "Gentile {{RecipientName}},\n\n" +
                      "La informiamo che sono previsti i seguenti interventi di manutenzione:\n\n" +
                      "{{CommunicationBody}}\n\n" +
                      "Ci scusiamo per gli eventuali disagi.\n\n" +
                      "Cordiali saluti,\n{{AdministratorName}}\n{{AdministratorEmail}}\n{{AdministratorPhone}}" },
            new { Type = "Emergency",    Name = "Comunicazione urgente",
                  Subject = "URGENTE — {{CommunicationTitle}} — {{CondominiumName}}",
                  Body =
                      "Gentile {{RecipientName}},\n\n" +
                      "COMUNICAZIONE URGENTE\n\n" +
                      "{{CommunicationBody}}\n\n" +
                      "La preghiamo di prenderne immediata visione.\n\n" +
                      "{{AdministratorName}}\n{{AdministratorEmail}}\n{{AdministratorPhone}}" },
            new { Type = "Info",         Name = "Comunicazione informativa",
                  Subject = "{{CommunicationTitle}} — {{CondominiumName}}",
                  Body =
                      "Gentile {{RecipientName}},\n\n" +
                      "La informiamo che:\n\n" +
                      "{{CommunicationBody}}\n\n" +
                      "Cordiali saluti,\n{{AdministratorName}}\n{{AdministratorEmail}}\n{{AdministratorPhone}}" },
        };

        var created = new List<NotificationTemplate>();
        foreach (var d in defaults)
        {
            // Salta i tipi per cui esiste già almeno un template default
            var alreadyExists = await session.Query<NotificationTemplate>()
                .AnyAsync(t => t.Condominium.Id == command.CondominiumId
                            && t.CommunicationType == d.Type
                            && t.IsDefault && !t.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (alreadyExists) continue;

            var tmpl = new NotificationTemplate
            {
                Condominium       = condominium,
                Tenant            = condominium.Tenant,
                Name              = d.Name,
                CommunicationType = d.Type,
                SubjectTemplate   = d.Subject,
                BodyTemplate      = d.Body,
                IsDefault         = true,
            };
            tmpl.Trace(currentUser);
            await session.SaveAsync(tmpl, cancellationToken).ConfigureAwait(false);
            created.Add(tmpl);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return created.Select(t => t.ToReadDto()).ToList();
    }
}
