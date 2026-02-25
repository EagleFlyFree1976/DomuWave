using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class Communication : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; }
        public virtual string Title { get; set; }
        public virtual string Content { get; set; }
        public virtual string CommunicationType { get; set; }
        public virtual string Priority { get; set; }
        public virtual DateTime PublicationDate { get; set; }
        public virtual DateTime? ExpirationDate { get; set; }
        public virtual bool SendEmail { get; set; }
        public virtual DateTime? EmailSentAt { get; set; }
        public virtual bool IsVisible { get; set; }
        public virtual string AttachmentPath { get; set; }

        public virtual IList<CommunicationRead> Reads { get; set; } = new List<CommunicationRead>();
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
