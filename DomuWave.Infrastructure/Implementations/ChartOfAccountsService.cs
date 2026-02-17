using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Linq;
using DomuWave.Common;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class ChartOfAccountsService : BaseService<ChartOfAccounts, int>, IChartOfAccountsService
    {
        public ChartOfAccountsService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "ChartOfAccountss";
        
        // Implement all interface methods using async NHibernate methods
        // Example methods follow the same pattern as above services
    }
}
