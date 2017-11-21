using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Hoard.BC.Contracts;
using System.Threading;
using Nethereum.Hex.HexTypes;

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

                    {
                        GameContract game = new GameContract(web, desc.GameContract);
                        
                        //bool ret = await game.SetGameServerURLAsync(this, @"http://ec2-52-57-192-150.eu-central-1.compute.amazonaws.com:8000");
                        string url = await game.GetGameServerURLAsync();

                        //TODO: get name of this game
                        //var gInfo = await gameCenter.GetGameInfoAsync(gameID);
                        desc.Url = url;
                        desc.PublicKey = "";//TODO: get it from BC somehow
                    }

                    return desc;
                }
            }
            catch(Exception)
            {
            }
            return null;
        }

        public async Task<bool> AddGame()
        {
            bool added = await gameCenter.AddGameAsync(this, 0, "myGame", "0xa0464599df2154ec933497d712643429e81d4628");
            return added;
        }

        /// <summary>
        /// temporary function to call functions on blockchain
        /// </summary>
        /// <returns></returns>
        public async Task<bool> EvaluateOnBC(Func<string, Task<string>> job)
        {
            string pw = "dev";
            string address = "0xa0464599df2154ec933497d712643429e81d4628";// Nethereum.Signer.EthECKey.GetPublicAddress(privateKey); //could do checksum
            var accountUnlockTime = 120;
            var unlockResult = await web.Personal.UnlockAccount.SendRequestAsync(address, pw, accountUnlockTime);

            Task<string> ts = job(address);// function.SendTransactionAsync(address, new HexBigInteger(4700000), new HexBigInteger(0), id, name, owner);

            var txHash = await ts;

            var receipt = await web.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            while (receipt == null)
            {
                Thread.Sleep(1000);
                receipt = await web.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            }

            return ulong.Parse(receipt.Status.ToString()) == 1;
        }
    }
}
