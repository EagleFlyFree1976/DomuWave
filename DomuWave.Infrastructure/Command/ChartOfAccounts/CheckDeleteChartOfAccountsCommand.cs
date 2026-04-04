using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccounts;

/// <summary>Risultato del controllo pre-cancellazione di un conto del piano dei conti.</summary>
public class CheckDeleteChartOfAccountsResult
{
    /// <summary>True se il conto ha sotto-conti: la cancellazione è bloccata.</summary>
    public bool HasChildren { get; set; }

    /// <summary>True se il conto è usato in voci di budget in stato Bozza.</summary>
    public bool HasDraftUsages { get; set; }

    /// <summary>True se il conto è usato in voci di budget Approvati o Chiusi.</summary>
    public bool HasLockedUsages { get; set; }

    /// <summary>
    /// True se è necessario selezionare un conto sostitutivo prima di procedere
    /// (ovvero HasDraftUsages == true).
    /// </summary>
    public bool RequiresReplacement => HasDraftUsages;

    /// <summary>True se la cancellazione è possibile (non ci sono sotto-conti).</summary>
    public bool CanDelete => !HasChildren;
}

public class CheckDeleteChartOfAccountsCommand : BaseCommand, IQuery<CheckDeleteChartOfAccountsResult>
{
    public int Id { get; set; }

    public CheckDeleteChartOfAccountsCommand() { }
    public CheckDeleteChartOfAccountsCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
