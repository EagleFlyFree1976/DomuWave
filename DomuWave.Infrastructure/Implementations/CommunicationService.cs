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
    public class CommunicationService : BaseService<Communication, int>, ICommunicationService
    {
        public CommunicationService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "Communications";
        
        // All methods use async NHibernate: FirstOrDefaultAsync, ToListAsync, CountAsync
    }
}
