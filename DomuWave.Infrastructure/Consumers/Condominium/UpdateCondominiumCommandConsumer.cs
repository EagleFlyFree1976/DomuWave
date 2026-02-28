using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateCondominiumCommandConsumer : InMemoryConsumerBase<UpdateCondominiumCommand, CondominiumReadDto>
{
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService _userService;

    public UpdateCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService = userService;
    }

    protected override async Task<CondominiumReadDto> Consume(
        UpdateCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var exists = await _condominiumService
            .ExistsAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (!exists) return null;

        var existing = await _condominiumService
            .GetByIdAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);


        if (string.IsNullOrEmpty(command.Dto.Code.Trim()))
        {
            throw new ValidatorException($"Specificare il codice condominio");
        }

        var existsCode = await session.Query<Condominium>()
            .Where(k => k.Code == command.Dto.Code.Trim() && k.Tenant.Id == existing.Tenant.Id && k.Id != existing.Id).AnyAsync(cancellationToken).ConfigureAwait(false);

        if (existsCode)
        {
            throw new ValidatorException($"Esiste già un condominio con il codice {command.Dto.Code}");
        }


        existing.ApplyUpdate(command.Dto);

        var updated = await _condominiumService
            .UpdateAsync(existing, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
