using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomuWave.Services.Models.Dto
{
    public class BookDto
    {
        public bool IsPrimary { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OwnerId { get; set; }
        
        public int? CurrencyId { get; set; }
    }


   
}
