using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Persistence.SessionFactories;
using NHibernate.Linq;
using DomuWave.Common;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class MessageService : BaseService<Message, long>, IMessageService
    {
        public MessageService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "Messages";
        
        // All methods use async NHibernate: FirstOrDefaultAsync, ToListAsync, CountAsync
    }
}
