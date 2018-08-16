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
        public string AccountsDir { get; set; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hoard", "acounts");
        public string DefaultAccountPass { get; set; } = "dev";
        public string GameCenterContract { get; set; }  = "0x9d4acb1e424d5eb00dac674ff5f59df7a9fac2b9";
        public bool PrivateNetwork { get; set; } = true;
    }
}
