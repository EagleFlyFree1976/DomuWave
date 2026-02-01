using DomuWave.Services.Command.Category;
using DomuWave.Services.Command.Category;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Category;
using DomuWave.Services.Models.Dto.Category;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Category;
 

/// <summary>
/// Crea un nuovo metodo di pagamento con im parametri impostati
/// </summary>
public class UpdateCategoryCommandConsumer : InMemoryConsumerBase<UpdateCategoryCommand, CategoryReadDto>
{
    private IUserService _userService;
    private ICategoryService _CategoryService;

    public UpdateCategoryCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, ICategoryService CategoryService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _CategoryService = CategoryService;
    }

    protected override async Task<CategoryReadDto> Consume(UpdateCategoryCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        Models.Category x = await _CategoryService.Update(evt.CategoryId,evt.UpdateDto, currentUser, cancellationToken).ConfigureAwait(false);
        return x.ToDto();

    }
}