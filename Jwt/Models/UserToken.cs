using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jwt.Models
{
    public class UserToken
    {
        public string Token { get; set; }
        public string Expiration { get; set; }
    }
}
