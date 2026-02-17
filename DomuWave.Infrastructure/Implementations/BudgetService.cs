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
    public class BudgetService : BaseService<Budget, int>, IBudgetService
    {
        public BudgetService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "Budgets";
        
        // Implement all interface methods using async NHibernate methods
        // Example methods follow the same pattern as above services
    }
}
