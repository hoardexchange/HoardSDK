using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public class HoardServiceOptions
    {
        public ulong GameID { get; set; }
        public string GameBackendUrl { get; set; } = "";
        public string BlockChainClientUrl { get; set; } = "";
        public string AccountsDir { get; set; } = "";
        public string DefaultAccountPass { get; set; } = "dev";
    }
}
