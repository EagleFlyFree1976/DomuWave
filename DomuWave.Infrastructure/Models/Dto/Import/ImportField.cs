using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomuWave.Services.Models.Dto.Import
{
    public class ImportField
    {
        public int FieldIndex { get; set; }
        public string FieldName { get; set; }
        public TargetField? TargetField { get; set; }

    }
}
