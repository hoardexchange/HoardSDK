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

        private const string GameInfoAddress = "0x1ea8a28bc9908a224f68870cf333eac9086f4118";

        public BCComm(string bcClientUrl)
        {
            web = new Web3(bcClientUrl);

            gameCenter = new GameCenterContract(web, GameInfoAddress);
        }

        public async Task<GBDesc> GetGBDesc(ulong gameID)
        {
            bool exist = await gameCenter.GetGameExistsAsync(gameID);
            if (exist)
            {
                GBDesc desc = new GBDesc();                
                desc.GameContract = await gameCenter.GetGameContractAsync(gameID);

                //bool added = await gameCenter.AddGameAsync(1, "myGame", desc.GameContract);

                var gInfo = await gameCenter.GetGameInfoAsync(gameID);
                desc.Url = System.Text.Encoding.UTF8.GetString(gInfo.Name);
                desc.PublicKey = "";//TODO: get it from BC somehow

                return desc;
            }
            return null;
        }
    }
}
