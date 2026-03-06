using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CondominiumFee;
using DomuWave.Services.Dto.CondominiumFee;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateCondominiumFeeCommandConsumer : InMemoryConsumerBase<CreateCondominiumFeeCommand, CondominiumFeeReadDto>
{
    private readonly ICondominiumFeeService _condominiumFeeService;
    private readonly IUserService           _userService;

    public CreateCondominiumFeeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumFeeService  condominiumFeeService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _condominiumFeeService = condominiumFeeService;
        _userService           = userService;
    }

    protected override async Task<CondominiumFeeReadDto> Consume(
        CreateCondominiumFeeCommand command,
        IMediationContext            mediationContext,
        CancellationToken            cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var dto = command.Dto;
        var installment = await session.Query<CondominiumInstallment>()
            .FirstOrDefaultAsync(x => x.Id == dto.InstallmentId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var unit = await session.Query<RealEstateUnit>()
            .FirstOrDefaultAsync(x => x.Id == dto.UnitId && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var entity  = dto.ToEntity(installment, unit);
        var created = await _condominiumFeeService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
