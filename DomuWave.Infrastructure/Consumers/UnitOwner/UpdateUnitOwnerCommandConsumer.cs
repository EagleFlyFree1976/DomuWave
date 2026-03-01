using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Clients;
using DomuWave.Services.Clients.Request;
using DomuWave.Services.Command.UnitOwner;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateUnitOwnerCommandConsumer : InMemoryConsumerBase<UpdateUnitOwnerCommand, UnitOwnerReadDto>
{
    private readonly IUnitOwnerService    _unitOwnerService;
    private readonly IUserService         _userService;
    
    private readonly IAuthorizationClient _authorizationClient;

    public UpdateUnitOwnerCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUnitOwnerService unitOwnerService,
        IUserService userService,
    
        IAuthorizationClient authorizationClient) : base(sessionFactoryProvider)
    {
        _unitOwnerService    = unitOwnerService;
        _userService         = userService;
    
        _authorizationClient = authorizationClient;
    }

    protected override async Task<UnitOwnerReadDto> Consume(
        UpdateUnitOwnerCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var existing = await _unitOwnerService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null) return null;

        existing.ApplyUpdate(command.Dto);

        var updated = await _unitOwnerService
            .UpdateAsync(existing, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Sync name / email back to the auth user record via Refit
        if (existing.UserId > 0)
        {
            
                await _authorizationClient.UpdateUserAsync(
                    currentUser.Token,
                    (int)existing.UserId,
                    new UpdateAuthUserRequest
                    {
                        Name     = command.Dto.FirstName,
                        SurName  = command.Dto.LastName,
                        Email    = command.Dto.Email
                        
                    },
                    cancellationToken).ConfigureAwait(false);
            
        }

        return updated.ToReadDto();
    }
}
