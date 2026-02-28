using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomuWave.Services.Command.Menu;
using DomuWave.Services.Models;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Menu
{
    public class PopulateMenuEventCommand : BaseTenantRelatedCommand, IQuery<IList<MenuItemDto>>
    {
        public PopulateMenuEventCommand()
        {
        }

        public PopulateMenuEventCommand(int currentUserId, Guid currentTenantId) : base(currentUserId, currentTenantId)
        {
        }

        public Guid? TenantId { get; set; }
    }
}
