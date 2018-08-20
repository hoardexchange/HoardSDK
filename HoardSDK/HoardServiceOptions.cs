using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Configuration params for HoardService
    /// </summary>
    public class HoardServiceConfig
    {
        public string GameID { get; set; } = null;
        public string GameBackendUrl { get; set; } = "";
        public string ClientUrl { get; set; } = "";
        public string AccountsDir { get; set; } = null;
        public string GameCenterContract { get; set; } = "0x9d4acb1e424d5eb00dac674ff5f59df7a9fac2b9";

        public static HoardServiceConfig Load(string path = null)
        {
            string defaultPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hoard", "hoardConfig.json");
            string cfgString = null;
            if (!string.IsNullOrEmpty(path))
            {
                cfgString = System.IO.File.ReadAllText(path);
            }
            else if (System.IO.File.Exists("hoardConfig.json"))
            {
                cfgString = System.IO.File.ReadAllText("hoardConfig.json");
            }
            else if (System.IO.File.Exists(defaultPath))
            {
                cfgString = System.IO.File.ReadAllText(defaultPath);
            }

            HoardServiceConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<HoardServiceConfig>(cfgString);
            return config;
        }
    }
    /// <summary>
    /// Initialization options for HoardService
    /// </summary>
    public class HoardServiceOptions
    {
        public GameID Game { get; set; } = GameID.kInvalidID;
        public Nethereum.JsonRpc.Client.IClient RpcClient { get; set; } = null;
        public string AccountsDir { get; set; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Hoard", "acounts");
        public string GameCenterContract { get; set; } = "";
        //TODO: remove this! this is for development purposes
        public string DefaultAccountPass { get; set; } = "dev";

        public HoardServiceOptions() { }

        public HoardServiceOptions(HoardServiceConfig cfg, Nethereum.JsonRpc.Client.IClient rpcClient)
        {
            if (string.IsNullOrEmpty(cfg.GameID))
            {
                Game = GameID.kInvalidID;
                Game.Url = cfg.GameBackendUrl;
            }
            else
            {
                Game = new GameID(cfg.GameID);
            }
            
            if (!string.IsNullOrEmpty(cfg.AccountsDir))
                AccountsDir = cfg.AccountsDir;

            RpcClient = rpcClient;

            GameCenterContract = cfg.GameCenterContract;
        }
    }
}
