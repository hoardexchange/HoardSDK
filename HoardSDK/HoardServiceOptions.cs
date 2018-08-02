using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Initialization options for HoardService
    /// </summary>
    public class HoardServiceOptions
    {
        public GameID Game { get; set; } = GameID.kInvalidID;
        public string GameBackendUrl { get; set; } = "";
        public Nethereum.JsonRpc.Client.IClient RpcClient { get; set; } = null;
        public string AccountsDir { get; set; } = "";
        public string DefaultAccountPass { get; set; } = "dev";
        public string GameCenterContract { get; set; }  = "0x672e3249f4e3674d52f446f7191e76b1294fcd33";
        public bool PrivateNetwork { get; set; } = true;
    }
}
