using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPQ.Core.Services;

namespace DomuWave.Services.Clients.Response
{
    public  class UserAuthorizationResponse
    {
        public string AuthCode { get; set; }
        public CanAuthorization Can { get; set; }
    }
}
