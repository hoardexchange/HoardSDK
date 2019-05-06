using System;
using System.Globalization;

namespace Hoard
{
    /// <summary>
    /// Configuration params for HoardService
    /// </summary>
    [System.Serializable]
    public class HoardServiceConfig
    {
        /// <summary>
        /// Game identifier in the form of hex BigInteger
        /// </summary>
        public string GameID;
        /// <summary>
        /// URL of GameServer
        /// </summary>
        public string GameBackendUrl;
        /// <summary>
        /// Blockchain client configuration
        /// </summary>
        public BCClientConfig BCClient;
        /// <summary>
        /// Whisper web socket server
        /// </summary>
        public string WhisperAddress;
        /// <summary>
        /// The address of main Hoard Game Center contract
        /// </summary>
        public string GameCenterContract;
        /// <summary>
        /// URL of Hoard Exchange services
        /// </summary>
        public string ExchangeServiceUrl;
        /// <summary>
        /// URL of Hoard authentication services [Optional]
        /// TODO: move this to provider specific settings
        /// </summary>
        public string HoardAuthServiceUrl;
        /// ID of game client for authentication purposes [Optional]
        /// /// TODO: move this to provider specific settings
        public string HoardAuthServiceClientId;
        /// Default directory with key store (for KeyStoreAccountProvider)
        /// TODO: move this to provider specific settings
        public string AccountsDir;

        /// <summary>
        /// Loads configuration from path on disk
        /// </summary>
        /// <param name="path">path where configration file is kept (hoardConfig.cfg)</param>
        /// <returns>new configuration object</returns>
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

        /// <summary>
        /// Loads configuration from supplied text stream
        /// </summary>
        /// <param name="data">JSON string representing configuration</param>
        /// <returns>new configuration object</returns>
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
        /// <summary>
        /// Game identifier
        /// </summary>
        public GameID Game { get; set; } = GameID.kInvalidID;
        /// <summary>
        /// Address of Hoard Game Contract
        /// </summary>
        public string GameCenterContract { get; set; } = "";
        /// <summary>
        /// URL of Hoard Exchange service
        /// </summary>
        public string ExchangeServiceUrl { get; set; } = "http://localhost:8000";
        /// <summary>
        /// URL of Hoard authentication services [Optional]
        /// TODO: move this to provider specific settings
        /// </summary>
        public string HoardAuthServiceUrl { get; set; } = "http://localhost:8081";
        /// ID of game client for authentication purposes [Optional]
        /// /// TODO: move this to provider specific settings
        public string HoardAuthServiceClientId { get; set; } = "HoardTestAuthClient";
        /// <summary>
        /// Blockchain client options
        /// </summary>
        public BCClientOptions BCClientOptions;

        /// <summary>
        /// Creates a new options object with default values
        /// </summary>
        public HoardServiceOptions() { }

        /// <summary>
        /// Creates new options objects with values parsed from string configuration data
        /// </summary>
        /// <param name="cfg">configuration file</param>
        /// <param name="bcClientOptions"></param>
        public HoardServiceOptions(HoardServiceConfig cfg, BCClientOptions bcClientOptions)
        {
            Game = GameID.kInvalidID;
            if (!string.IsNullOrEmpty(cfg.GameID))
                Game = new GameID(System.Numerics.BigInteger.Parse(cfg.GameID, NumberStyles.AllowHexSpecifier));

            if (!string.IsNullOrEmpty(cfg.GameBackendUrl))
                Game.Url = cfg.GameBackendUrl;

            if (!string.IsNullOrEmpty(cfg.ExchangeServiceUrl))
                ExchangeServiceUrl = cfg.ExchangeServiceUrl;

            if (!string.IsNullOrEmpty(cfg.HoardAuthServiceUrl))
                HoardAuthServiceUrl = cfg.HoardAuthServiceUrl;

            if (!string.IsNullOrEmpty(cfg.HoardAuthServiceClientId))
                HoardAuthServiceClientId = cfg.HoardAuthServiceClientId;

            BCClientOptions = bcClientOptions;

            GameCenterContract = cfg.GameCenterContract;
        }
    }
}
