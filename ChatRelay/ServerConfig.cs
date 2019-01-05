using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatRelay
{
    public class ServerConfig
    {
        public List<Server> Servers { get; set; } = new List<Server>();
        public bool RconOnly { get; set; } = false;
    }
}
