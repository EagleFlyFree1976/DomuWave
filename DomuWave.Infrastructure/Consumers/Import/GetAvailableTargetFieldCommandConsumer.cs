using DomuWave.Services.Command.Import;
using DomuWave.Services.Helper;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Import;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.Category;

public class GetAvailableTargetFieldCommandConsumer : InMemoryConsumerBase<GetAvailableTargetFieldCommand, IList<TargetField>>
{
   
    private IImportService _importService;

    public GetAvailableTargetFieldCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IImportService importService) : base(sessionFactoryProvider)
    {
        _importService = importService;
    }

    protected override async Task<IList<TargetField>> Consume(GetAvailableTargetFieldCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
         
        List<TargetField> targets = new List<TargetField>();
        targets.Add(new TargetField(){Code = TargetFieldCodes.TransactionDate, Label = "Data transazione", Mandatory = true});
        targets.Add(new TargetField(){Code = TargetFieldCodes.DepositAmount, Label = "Importo in entrata", Mandatory = false});
        targets.Add(new TargetField(){Code = TargetFieldCodes.WithdrawalAmount, Label = "Importo in uscita", Mandatory = false});
        targets.Add(new TargetField(){Code = TargetFieldCodes.CategoryName, Label = "Categoria", Mandatory = false});
        targets.Add(new TargetField(){Code = TargetFieldCodes.SubCategoryName, Label = "Sotto Categoria", Mandatory = false});
        targets.Add(new TargetField(){Code = TargetFieldCodes.Description, Label = "Note", Mandatory = false});
        targets.Add(new TargetField(){Code = TargetFieldCodes.Currency, Label = "Valuta", Mandatory = false});
        targets.Add(new TargetField(){Code = TargetFieldCodes.Amount, Label = "Importo transazione (valore assoluto)", Mandatory = false});
        targets.Add(new TargetField(){Code = TargetFieldCodes.Beneficiary, Label = "Beneficiario", Mandatory = false});
        targets.Add(new TargetField(){Code = TargetFieldCodes.Type, Label = "Tipo transazione", Mandatory = true});
        targets.Add(new TargetField(){Code = TargetFieldCodes.Status, Label = "Stato transazione", Mandatory = false});
        

        return targets;
    }
}


