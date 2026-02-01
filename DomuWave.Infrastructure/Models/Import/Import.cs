using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomuWave.Services.Models.Import
{
    public class Import : BookEntity<long>
    {
        public virtual string Name { get; set; }
        public virtual Account TargetAccount { get; set; }
        
        public virtual string ContentType { get; set; }
        public virtual byte[] FileData { get; set; }
        public virtual string Configuration { get; set; }
        

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
