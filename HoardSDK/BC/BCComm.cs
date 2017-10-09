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
        private GameInfoContract gameInfo = null;

        private string GameInfoAddress = "0x1ea8a28bc9908a224f68870cf333eac9086f4118";

        public BCComm(string bcClientUrl)
        {
            web = new Web3(bcClientUrl);

            gameInfo = new GameInfoContract(web, GameInfoAddress);
        }

        public async Task<string> GetGBUrl(ulong gameID)
        {
            bool exist = await gameInfo.GetGameExistsAsync(gameID);
            if (exist)
            {
                //var gInfo = await gameInfo.GetGameInfoAsync(gameID);
                //return gInfo.Name;
                var gInfo = await gameInfo.GetGameContact(gameID);
                return gInfo.ToString();
            }
            return null;
        }
    }
}
