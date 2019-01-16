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
    [System.Serializable]
    public class HoardServiceConfig
    {
        public string GameID;
        public string GameBackendUrl;
        public string ClientUrl;
        public string GameCenterContract;
        public string ExchangeServiceUrl;
        public string HoardAuthServiceUrl;
        public string HoardAuthServiceClientId;
        public string AccountsDir;

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

        public static HoardServiceConfig LoadFromStream(string data)
        {
            HoardServiceConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<HoardServiceConfig>(data);
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
        public string GameCenterContract { get; set; } = "";
        public string ExchangeServiceUrl { get; set; } = "http://localhost:8000";
        public string HoardAuthServiceUrl { get; set; } = "http://localhost:8081";
        public string HoardAuthServiceClientId { get; set; } = "HoardTestAuthClient";
        //public IUserInputProvider UserInputProvider { get; set; } = null;

        /// <summary>
        /// Creates a new options object with default values
        /// </summary>
        public HoardServiceOptions() { }

        /// <summary>
        /// Creates new options objects with values parsed from string configuration data
        /// </summary>
        /// <param name="cfg">configuration file</param>
        /// <param name="rpcClient"></param>
        public HoardServiceOptions(HoardServiceConfig cfg, Nethereum.JsonRpc.Client.IClient rpcClient)
        {
            if (string.IsNullOrEmpty(cfg.GameID))
            {
                Game = GameID.kInvalidID;
                Game.Url = cfg.GameBackendUrl;
            }
            else
            {
                Game = new GameID(System.Numerics.BigInteger.Parse(cfg.GameID, System.Globalization.NumberStyles.HexNumber));
            }

            if (!string.IsNullOrEmpty(cfg.ExchangeServiceUrl))
                ExchangeServiceUrl = cfg.ExchangeServiceUrl;

            if (!string.IsNullOrEmpty(cfg.HoardAuthServiceUrl))
                HoardAuthServiceUrl = cfg.HoardAuthServiceUrl;

            if (!string.IsNullOrEmpty(cfg.HoardAuthServiceClientId))
                HoardAuthServiceClientId = cfg.HoardAuthServiceClientId;

            RpcClient = rpcClient;

            GameCenterContract = cfg.GameCenterContract;
        }
    }
}
