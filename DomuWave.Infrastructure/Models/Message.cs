using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class Message : TenantEntity<long>
    {
        public virtual Condominium Condominium { get; set; } = null!;
        public virtual long SenderId { get; set; }
        public virtual long RecipientId { get; set; }
        public virtual string Subject { get; set; } = string.Empty;
        public virtual string Content { get; set; } = string.Empty;
        public virtual Message? ParentMessage { get; set; }
        public virtual bool IsRead { get; set; }
        public virtual DateTime? ReadDate { get; set; }
        public virtual string? AttachmentPath { get; set; }

        public virtual IList<Message> Replies { get; set; } = new List<Message>();
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
