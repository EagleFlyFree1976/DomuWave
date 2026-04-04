using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccounts;

public class DeleteChartOfAccountsCommand : BaseCommand, IQuery<bool>
{
    public int  Id { get; set; }

    /// <summary>
    /// Obbligatorio se il conto è usato in voci di budget in stato Bozza.
    /// Le voci e le spese di quei budget vengono riassegnate a questo conto sostitutivo.
    /// Può essere null se le uniche occorrenze sono in budget Approvati o Chiusi
    /// (in quel caso vengono lasciate agganciare al conto cancellato logicamente).
    /// </summary>
    public int? ReplacementAccountId { get; set; }

    public DeleteChartOfAccountsCommand() { }
    public DeleteChartOfAccountsCommand(int currentUserId, int id, int? replacementAccountId = null)
        : base(currentUserId)
    {
        Id                    = id;
        ReplacementAccountId  = replacementAccountId;
    }
}
