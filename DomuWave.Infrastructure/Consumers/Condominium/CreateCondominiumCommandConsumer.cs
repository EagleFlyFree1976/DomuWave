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
using Remotion.Linq.Parsing;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateCondominiumCommandConsumer : InMemoryConsumerBase<CreateCondominiumCommand, CondominiumReadDto>
{
    private readonly ICondominiumService _condominiumService;
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public CreateCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        ITenantService tenantService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _tenantService      = tenantService;
        _userService        = userService;
    }

    protected override async Task<CondominiumReadDto> Consume(
        CreateCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var tenant = await _tenantService
            .GetByIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (tenant == null)
            throw new NotFoundException($"Dati non validi");
        
        var entity = command.Dto.ToEntity(tenant);
        
        /* validazioni */
        // verifico l'univocit� del codice condominio

        if (string.IsNullOrEmpty(command.Dto.Code.Trim()))
        {
            throw new ValidatorException($"Specificare il codice condominio");
        }

        var existsCode = await session.Query<Condominium>()
            .Where(k => k.Code == command.Dto.Code.Trim() && k.Tenant.Id == command.TenantId && !k.IsDeleted).AnyAsync(cancellationToken).ConfigureAwait(false);

        if (existsCode)
        {
            throw new ValidatorException($"Esiste gi� un condominio con il codice {command.Dto.Code}");
        }


            var created = await _condominiumService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
