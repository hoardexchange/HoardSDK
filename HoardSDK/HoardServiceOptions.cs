using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public class HoardServiceOptions
    {
        public GameID Game { get; set; } = GameID.kInvalidID;
        public string GameBackendUrl { get; set; } = "";
        public Nethereum.JsonRpc.Client.IClient RpcClient { get; set; } = null;
        public string AccountsDir { get; set; } = "";
        public string DefaultAccountPass { get; set; } = "dev";
        public bool PrivateNetwork { get; set; } = true;
    }
}
