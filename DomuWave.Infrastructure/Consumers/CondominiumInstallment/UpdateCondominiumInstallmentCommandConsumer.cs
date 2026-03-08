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

public class UpdateCondominiumInstallmentCommandConsumer : InMemoryConsumerBase<UpdateCondominiumInstallmentCommand, CondominiumInstallmentReadDto>
{
    private readonly ICondominiumInstallmentService _condominiumInstallmentService;
    private readonly IUserService                   _userService;

    public UpdateCondominiumInstallmentCommandConsumer(
        ISessionFactoryProvider        sessionFactoryProvider,
        ICondominiumInstallmentService condominiumInstallmentService,
        IUserService                   userService) : base(sessionFactoryProvider)
    {
        _condominiumInstallmentService = condominiumInstallmentService;
        _userService                   = userService;
    }

    protected override async Task<CondominiumInstallmentReadDto> Consume(
        UpdateCondominiumInstallmentCommand command,
        IMediationContext                   mediationContext,
        CancellationToken                   cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _condominiumInstallmentService
            .GetByIdAsync(command.InstallmentId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null) return null;

        var status = await session.Query<CondominiumInstallmentStatus>()
            .FirstOrDefaultAsync(x => x.Id == command.Dto.StatusId, cancellationToken)
            .ConfigureAwait(false);

        entity.ApplyUpdate(command.Dto, status);
        var updated = await _condominiumInstallmentService
            .UpdateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
