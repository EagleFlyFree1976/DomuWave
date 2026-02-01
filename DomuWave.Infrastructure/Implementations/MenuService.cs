using DomuWave.Services.Clients;
using DomuWave.Services.Command.Menu;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Implementations
{
    public class MenuService : BaseService, IMenuService
    {
        protected IAuthorizationClient _authorizationClient;
        protected IMediator _mediator;

        public override string CacheRegion
        {
            get { return "Menu"; }
        }

        public MenuService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache, IAuthorizationClient authorizationClient, IMediator mediator) : base(sessionFactoryProvider, cache)
        {
            _authorizationClient = authorizationClient;
            _mediator = mediator;
        }

        public async Task<IList<MenuItem>> GetAllMenuItems(IUser currentUser, long? bookId, CancellationToken cancellationToken)
        {
            var allMenuItems = await session.Query<MenuItem>().OrderBy(k=>k.OrderKey).ToListAsync(cancellationToken).ConfigureAwait(false);

            var allUserAuthorization = await _authorizationClient
                .GetAllUserAuthorizations(currentUser.Token, currentUser.Id, cancellationToken).ConfigureAwait(false);

            var authCodes = allUserAuthorization.Select(a => a.AuthCode).ToHashSet();

            var authorizedMenuItems = allMenuItems
                .Where(item => authCodes.Contains(item.AuthorizationCode) || string.IsNullOrEmpty(item.AuthorizationCode))
                .ToList();

          
            return authorizedMenuItems;
        }


        
    }
}
