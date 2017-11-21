using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Hoard.BC.Contracts;

namespace Hoard.BC
{
    /// <summary>
    /// Internal class for Blockchain communication.
    /// Uses Nethereum library.
    /// </summary>
    class BCComm
    {
        private Web3 web = null;
        private GameCenterContract gameCenter = null;

        //private const string GameInfoAddress = "0x846f3a06aa6bde218e5f966d91e3e4d4ae2bd3ec";
        private const string GameInfoAddress = "0x846f3a06aa6bde218e5f966d91e3e4d4ae2bd3ec";

        public BCComm(string bcClientUrl)
        {
            web = new Web3(bcClientUrl);

            gameCenter = new GameCenterContract(web, GameInfoAddress);
        }

        public async Task<string> Connect()
        {
            var ver = new Nethereum.RPC.Web3.Web3ClientVersion(web.Client);
            return await ver.SendRequestAsync();
        }

        public async Task<GBDesc> GetGBDesc(ulong gameID)
        {
            try
            {
                bool exist = await gameCenter.GetGameExistsAsync(gameID);
                if (exist)
                {
                    GBDesc desc = new GBDesc();
                    desc.GameContract = await gameCenter.GetGameContractAsync(gameID);

                    var gInfo = await gameCenter.GetGameInfoAsync(gameID);
                    desc.Url = System.Text.Encoding.UTF8.GetString(gInfo.Name);
                    desc.PublicKey = "";//TODO: get it from BC somehow

                    return desc;
                }
            }
            catch(Exception)
            {
            }
            return null;
        }

        public async Task<string> AddGame()
        {
            string added = await gameCenter.AddGameAsync(0, "myGame", "0xa0464599df2154ec933497d712643429e81d4628");
            return added;
        }
    }
}
