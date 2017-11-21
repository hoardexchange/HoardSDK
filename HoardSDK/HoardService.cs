using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hoard
{
    public class HoardService
    {
        public GBDesc GameBackendDesc { get; private set;}
        public bool IsSingedIn { get; private set;}

        private BC.BCComm bcComm = null;
        private Exception Exception = null;

        public HoardService()
        {
            
        }

        /// <summary>
        /// Connects to BC and fills missing options
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<bool> Init(HoardServiceOptions options)
        {
            bcComm = new BC.BCComm(options.BlockChainClientUrl);
            string connectionResponse = await bcComm.Connect();

            GBDesc gbDesc = await bcComm.GetGBDesc(options.GameID);
            if (gbDesc == null)
            {
                bool p = await bcComm.AddGame();
                gbDesc = new GBDesc();
                gbDesc.Url = options.GameBackendUrl;
                return false;
            }

            GameBackendDesc = gbDesc;

            return true;
        }

        public async Task<ItemID[]> RequestItemList()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SignIn(PlayerID id)
        {
            //create hoard client
            Client client = new Client(GameBackendDesc);
            //connect to backend
            return await client.Connect(id);
        }

        public async Task<ItemCRC[]> RequestItemsCRC(ItemID[] items)
        {
            throw new NotImplementedException();
        }

        public async Task<ItemData> RequestItemData(ItemID id)
        {
            throw new NotImplementedException();
        }
    }
}
