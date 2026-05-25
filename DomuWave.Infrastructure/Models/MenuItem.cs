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

        public virtual string? Icon { get; set; }
        public virtual string? Description { get; set; }
        public virtual string Action { get; set; } = string.Empty;

        public virtual string AuthorizationCode { get; set; } = string.Empty;
        public virtual string? PopulateEvent { get; set; }

        public virtual int OrderKey { get; set; }



        /// <summary>
        /// Etichette aggiuntive separate da virgola (es. "admin,report,beta")
        /// </summary>
        public virtual string? Tags { get; set; }

        /// <summary>
        /// Feature code separati da virgola richiesti per visualizzare la voce di menu.
        /// Se null o vuoto, la voce è sempre visibile.
        /// </summary>
        public virtual string? Features { get; set; }
    }
}
