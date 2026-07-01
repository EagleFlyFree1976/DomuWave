using System;
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

        // ── Sondaggio ───────────────────────────────────────────────────────────
        // Un sondaggio è un BoardPost con IsPoll=true e una lista di Options.
        public virtual bool IsPoll { get; set; }
        // Se true: si mostrano solo i conteggi, mai chi ha votato cosa.
        public virtual bool IsAnonymous { get; set; }
        // Se true: il votante può selezionare più opzioni.
        public virtual bool AllowMultiple { get; set; }
        // Scadenza opzionale oltre cui non si può più votare.
        public virtual DateTime? ClosesAt { get; set; }

        public virtual IList<BoardPostComment> Comments { get; set; } = new List<BoardPostComment>();
        public virtual IList<BoardPostOption> Options { get; set; } = new List<BoardPostOption>();
        public virtual IList<BoardPostVote> Votes { get; set; } = new List<BoardPostVote>();

        public override int GetHashCode() => Id.GetHashCode();
    }
}
