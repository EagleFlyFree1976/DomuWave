using System;
using System.Collections.Generic;
using DomuWave.Services.Dto.ElectronicInvoice;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ElectronicInvoices;

/// <summary>
/// Scarica massivamente le fatture elettroniche passive del condominio dal provider SdI
/// nell'intervallo indicato, deduplica su SdiIdentifier e le persiste. Restituisce le
/// fatture importate in questa esecuzione. Riservato all'amministratore.
/// </summary>
public class SyncEInvoicesCommand : BaseCommand, IQuery<IList<ElectronicInvoiceReadDto>>
{
    public int CondominiumId { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public SyncEInvoicesCommand() { }
    public SyncEInvoicesCommand(int currentUserId, int condominiumId, DateTime from, DateTime to)
        : base(currentUserId)
    {
        CondominiumId = condominiumId;
        From = from;
        To = to;
    }
}

/// <summary>
/// Restituisce la configurazione corrente del download fatture per un condominio
/// (provider, P.IVA, presenza chiave, ultima sync). Non espone la chiave API.
/// </summary>
public class GetEInvoiceConfigCommand : BaseCommand, IQuery<EInvoiceConfigReadDto>
{
    public int CondominiumId { get; set; }

    public GetEInvoiceConfigCommand() { }
    public GetEInvoiceConfigCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}

/// <summary>
/// Salva la configurazione del download fatture per un condominio. La chiave API viene
/// aggiornata solo se valorizzata (altrimenti resta invariata). Restituisce la config aggiornata.
/// </summary>
public class UpdateEInvoiceConfigCommand : BaseCommand, IQuery<EInvoiceConfigReadDto>
{
    public int CondominiumId { get; set; }
    public EInvoiceConfigUpdateDto Dto { get; set; }

    public UpdateEInvoiceConfigCommand() { }
    public UpdateEInvoiceConfigCommand(int currentUserId, int condominiumId, EInvoiceConfigUpdateDto dto)
        : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Dto = dto;
    }
}

/// <summary>
/// Restituisce le fatture elettroniche scaricate per un condominio (escluse le cancellate).
/// </summary>
public class GetEInvoicesByCondominiumCommand : BaseCommand, IQuery<IList<ElectronicInvoiceReadDto>>
{
    public int CondominiumId { get; set; }

    public GetEInvoicesByCondominiumCommand() { }
    public GetEInvoicesByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}

/// <summary>
/// Collega una fattura elettronica scaricata a una spesa esistente (o appena creata),
/// portandone lo stato a Linked. Restituisce la fattura aggiornata.
/// </summary>
public class LinkEInvoiceToExpenseCommand : BaseCommand, IQuery<ElectronicInvoiceReadDto>
{
    public int InvoiceId { get; set; }
    public long ExpenseId { get; set; }

    public LinkEInvoiceToExpenseCommand() { }
    public LinkEInvoiceToExpenseCommand(int currentUserId, int invoiceId, long expenseId) : base(currentUserId)
    {
        InvoiceId = invoiceId;
        ExpenseId = expenseId;
    }
}
