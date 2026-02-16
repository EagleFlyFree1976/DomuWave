using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomuWave.Services.Settings
{
    public class CacheTimeouts
    {
        public int Currency { get; set; } = 24 * 60;
        public int Tag { get; set; } = 24 * 60;
    }
}
