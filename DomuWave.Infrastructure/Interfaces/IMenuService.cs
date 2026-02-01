using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomuWave.Services.Models;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces
{
    public interface IMenuService 
    {
        Task<IList<MenuItem>> GetAllMenuItems(IUser currentUser,long? bookId, CancellationToken cancellationToken);
    }
}
