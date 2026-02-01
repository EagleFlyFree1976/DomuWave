using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.Category;
using DomuWave.Services.Models.Dto.PaymentMethod;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Category
{
    /// <summary>
    /// Crea una nuova categoria con im parametri impostati
    /// </summary>
    public class CreateCategoryCommandConsumer : InMemoryConsumerBase<CreateCategoryCommand, CategoryReadDto>
    {
        private IUserService _userService;
        private ICategoryService _CategoryService;

        public CreateCategoryCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, ICategoryService CategoryService) : base(sessionFactoryProvider)
        {
            _userService = userService;
            _CategoryService = CategoryService;
        }

        protected override async Task<CategoryReadDto> Consume(CreateCategoryCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
        {
            var currentUser =
                await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

            Models.Category x = await _CategoryService.Create(evt.CreateDto, evt.CreateDto.BookId, currentUser, cancellationToken).ConfigureAwait(false);
            return x.ToDto();

        }
    }

 

        
    
}
