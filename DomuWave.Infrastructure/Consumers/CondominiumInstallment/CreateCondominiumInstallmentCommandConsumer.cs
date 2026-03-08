using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumInstallment;
using DomuWave.Services.Dto.CondominiumInstallment;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateCondominiumInstallmentCommandConsumer : InMemoryConsumerBase<CreateCondominiumInstallmentCommand, CondominiumInstallmentReadDto>
{
    private readonly ICondominiumInstallmentService _condominiumInstallmentService;
    private readonly IUserService                   _userService;

    public CreateCondominiumInstallmentCommandConsumer(
        ISessionFactoryProvider         sessionFactoryProvider,
        ICondominiumInstallmentService  condominiumInstallmentService,
        IUserService                    userService) : base(sessionFactoryProvider)
    {
        _condominiumInstallmentService = condominiumInstallmentService;
        _userService                   = userService;
    }

    protected override async Task<CondominiumInstallmentReadDto> Consume(
        CreateCondominiumInstallmentCommand command,
        IMediationContext                   mediationContext,
        CancellationToken                   cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var dto = command.Dto;
        var condominium = await session.Query<Models.Condominium>()
            .FirstOrDefaultAsync(x => x.Id == dto.CondominiumId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        Budget? budget = null;
        if (dto.BudgetId.HasValue)
            budget = await session.Query<Budget>()
                .FirstOrDefaultAsync(x => x.Id == dto.BudgetId.Value && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

        FiscalYear? fiscalYear = null;
        if (dto.FiscalYearId.HasValue)
            fiscalYear = await session.Query<FiscalYear>()
                .FirstOrDefaultAsync(x => x.Id == dto.FiscalYearId.Value && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

        var status = await session.Query<CondominiumInstallmentStatus>()
            .FirstOrDefaultAsync(x => x.Id == dto.StatusId, cancellationToken)
            .ConfigureAwait(false);

        var entity  = dto.ToEntity(condominium, budget, fiscalYear, status);
        var created = await _condominiumInstallmentService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
