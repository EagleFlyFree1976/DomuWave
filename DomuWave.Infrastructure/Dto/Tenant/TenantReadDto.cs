using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Tenant
{
    public class TenantReadDto : TraceEntityDTO<long>
    {

        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsPrimary { get; set; }
        public long OwnerId { get; set; }

        public int CurrencyId { get; set; }
    }
}
