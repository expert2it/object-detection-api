using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.NetCore.Models
{
    
    public class Jwt
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
    }

}
