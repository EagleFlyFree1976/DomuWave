using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.MillesimalTable;
using DomuWave.Services.Dto.MillesimalTable;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateMillesimalTableCommandConsumer : InMemoryConsumerBase<UpdateMillesimalTableCommand, MillesimalTableReadDto>
{
    private readonly IMillesimalTableService _millesimalTableService;
    private readonly IUserService            _userService;

    public UpdateMillesimalTableCommandConsumer(
        ISessionFactoryProvider  sessionFactoryProvider,
        IMillesimalTableService  millesimalTableService,
        IUserService             userService) : base(sessionFactoryProvider)
    {
        _millesimalTableService = millesimalTableService;
        _userService            = userService;
    }

    protected override async Task<MillesimalTableReadDto> Consume(
        UpdateMillesimalTableCommand command,
        IMediationContext             mediationContext,
        CancellationToken             cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _millesimalTableService
            .GetByIdAsync(command.TableId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null) return null;

        var originalCode = entity.Code;

        // Impedisce di cambiare il codice della tabella di default
        if (entity.IsDefault && command.Dto.Code != originalCode)
            throw new ValidatorException("Il codice della tabella millesimale predefinita non può essere modificato.");

        // Verifica unicità del codice nel condominio (esclude la riga corrente)
        var newCode = command.Dto.Code?.Trim();
        if (!string.IsNullOrEmpty(newCode) && !string.Equals(newCode, originalCode, StringComparison.OrdinalIgnoreCase))
        {
            var duplicate = await session.Query<MillesimalTable>()
                .AnyAsync(t => t.Condominium.Id == entity.Condominium.Id
                            && t.Code == newCode
                            && t.Id != command.TableId
                            && !t.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (duplicate)
                throw new ValidatorException($"Esiste già una tabella millesimale con il codice '{newCode}' per questo condominio.");
        }

        entity.ApplyUpdate(command.Dto);
        var updated = await _millesimalTableService
            .UpdateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
