using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Import
{
    public class CreateImportCommand : BaseBookRelatedCommand, IQuery<Models.Import.Import>
    {
        public long TargetAccountId { get; set; }

        public string Name { get; set; }
        public string FileName { get; set; }

        public Stream csvStream { get; set; }

        public bool HasHeader { get; set; }
    }
}
