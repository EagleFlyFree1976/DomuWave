using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IBoardPostService : IBaseService<BoardPost, int>
{
    Task<IList<BoardPost>> GetByCondominiumAsync(int condominiumId, IUser currentUser, CancellationToken ct);
}

public interface IBoardPostCommentService : IBaseService<BoardPostComment, int>
{
    Task<IList<BoardPostComment>> GetByPostAsync(int boardPostId, IUser currentUser, CancellationToken ct);
}
