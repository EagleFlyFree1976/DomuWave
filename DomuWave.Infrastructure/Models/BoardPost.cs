using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class BoardPost : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; } = null!;
        public virtual long AuthorUserId { get; set; }
        public virtual string AuthorFullName { get; set; } = string.Empty;
        public virtual bool IsPinned { get; set; }

        public virtual IList<BoardPostComment> Comments { get; set; } = new List<BoardPostComment>();

        public override int GetHashCode() => Id.GetHashCode();
    }
}
