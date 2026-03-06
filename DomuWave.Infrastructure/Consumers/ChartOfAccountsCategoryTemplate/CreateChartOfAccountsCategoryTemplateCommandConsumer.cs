using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsCategoryTemplate;
using DomuWave.Services.Dto.ChartOfAccountsCategoryTemplate;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateChartOfAccountsCategoryTemplateCommandConsumer
    : InMemoryConsumerBase<CreateChartOfAccountsCategoryTemplateCommand, ChartOfAccountsCategoryTemplateReadDto>
{
    private readonly IChartOfAccountsCategoryTemplateService _templateService;
    private readonly IUserService                            _userService;

    public CreateChartOfAccountsCategoryTemplateCommandConsumer(
        ISessionFactoryProvider                  sessionFactoryProvider,
        IChartOfAccountsCategoryTemplateService  templateService,
        IUserService                             userService) : base(sessionFactoryProvider)
    {
        _templateService = templateService;
        _userService     = userService;
    }

    protected override async Task<ChartOfAccountsCategoryTemplateReadDto> Consume(
        CreateChartOfAccountsCategoryTemplateCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidatorException("Il nome del template è obbligatorio.");

        var nameExists = await session.Query<ChartOfAccountsCategoryTemplate>()
            .AnyAsync(x => x.Name == dto.Name.Trim() && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (nameExists)
            throw new ValidatorException($"Esiste già un template con nome '{dto.Name}'.");

        var entity  = dto.ToEntity();
        var created = await _templateService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return created.ToReadDto();
    }
}
