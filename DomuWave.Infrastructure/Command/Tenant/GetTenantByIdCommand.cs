using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomuWave.Services.Dto.Tenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Tenant
{
    public class GetTenantByIdCommand : BaseTenantRelatedCommand, IQuery<TenantReadDto>
    {
        public GetTenantByIdCommand()
        {
        }

        public GetTenantByIdCommand(int currentUserId, Guid tenantId) : base(currentUserId, tenantId)
        {
        }
    }
}
