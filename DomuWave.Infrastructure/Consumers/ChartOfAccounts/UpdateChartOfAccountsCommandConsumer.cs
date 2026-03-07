using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccounts;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateChartOfAccountsCommandConsumer
    : InMemoryConsumerBase<UpdateChartOfAccountsCommand, ChartOfAccountsReadDto>
{
    private readonly IChartOfAccountsService _accountService;
    private readonly IUserService            _userService;

    public UpdateChartOfAccountsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IChartOfAccountsService accountService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _accountService = accountService;
        _userService    = userService;
    }

    protected override async Task<ChartOfAccountsReadDto> Consume(
        UpdateChartOfAccountsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entity = await _accountService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null)
            throw new NotFoundException("Conto non trovato.");

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ValidatorException("Il codice conto è obbligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidatorException("Il nome conto è obbligatorio.");

        var codeExists = await session.Query<ChartOfAccounts>()
            .AnyAsync(x => x.Condominium.Id == entity.Condominium.Id
                        && x.Code == dto.Code.Trim()
                        && x.Id   != command.Id
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (codeExists)
            throw new ValidatorException($"Esiste già un conto con codice '{dto.Code}' per questo condominio.");

        ChartOfAccountsCategory? category = null;
        if (dto.CategoryId.HasValue)
        {
            category = await session.Query<ChartOfAccountsCategory>()
                .FirstOrDefaultAsync(x => x.Id == dto.CategoryId.Value && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (category == null)
                throw new NotFoundException("Categoria non trovata.");
        }

        MillesimalTable? defaultMillesimalTable = null;
        if (dto.DefaultMillesimalTableId.HasValue)
        {
            defaultMillesimalTable = await session.Query<MillesimalTable>()
                .FirstOrDefaultAsync(x => x.Id == dto.DefaultMillesimalTableId.Value && !x.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (defaultMillesimalTable == null)
                throw new NotFoundException("Tabella millesimale non trovata.");
        }

        entity.ApplyUpdate(dto, category, defaultMillesimalTable);
        var updated = await _accountService
            .UpdateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
