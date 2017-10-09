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
            string GBurl = await bcComm.GetGBUrl(options.GameID);
            if (GBurl == null)
                return false;

            GBDesc gbDesc = new GBDesc();
            gbDesc.Url = GBurl;

            GameBackendDesc = gbDesc;

            return true;
        }

        public async Task<ItemID[]> RequestItemList()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SignIn(PlayerID id)
        {
            throw new NotImplementedException();
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
