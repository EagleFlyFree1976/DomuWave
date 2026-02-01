using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPQ.Core;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace DomuWave.Services.Models
{
    public class MenuItem : IEntityBase<int>
    {
        public virtual  int Id { get; set; }

        public virtual int? ParentMenuId { get; set; }

        public virtual string Icon { get; set; }
        public virtual string Description { get; set; }
        public virtual string Action { get; set; }

        public virtual string AuthorizationCode { get; set; }
        public virtual string PopulateEvent { get; set; }

        public virtual int OrderKey { get; set; }
    }
}
