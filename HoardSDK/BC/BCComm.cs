using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Hoard.BC.Contracts;
using System.Threading;

namespace Hoard.BC
{
    /// <summary>
    /// Internal class for Blockchain communication.
    /// Uses Nethereum library.
    /// </summary>
    public class BCComm
    {
        private Web3 web = null;
        private GameCenterContract gameCenter = null;
        
        private const string GameInfoAddress = "0x0904edbaecaa7e3219104d151df6c4bb6f0b5b66";

        public BCComm(Nethereum.JsonRpc.Client.IClient client, Account account)
        {
            web = new Web3(account, client);

            gameCenter = GetContract<GameCenterContract>(GameInfoAddress);
        }

        public async Task<string> Connect()
        {
            var ver = new Nethereum.RPC.Web3.Web3ClientVersion(web.Client);
            return await ver.SendRequestAsync();
        }

        public TContract GetContract<TContract>(string contractAddress)
        {
            return (TContract)Activator.CreateInstance(typeof(TContract), web, contractAddress);
        }

        public async Task<GBDesc> GetGBDesc(ulong gameID)
        {
            bool exist = await gameCenter.GetGameExistsAsync(gameID);
            if (exist)
            {
                GBDesc desc = new GBDesc();
                desc.GameContract = await gameCenter.GetGameContractAsync(gameID);

                {
                    GameContract game = new GameContract(web, desc.GameContract);

                    string url = await game.GetGameServerURLAsync();

                    desc.GameID = gameID;
                    desc.Name = await game.Name();
                    desc.Url = !url.StartsWith("http") ? "http://" + url : url;
                }

                return desc;
            }
            return null;
        }

        public Task<ulong> GetGameAssetBalanceOf(string address, string tokenContractAddress)
        {
            GameAssetContract gameAsset = new GameAssetContract(web, tokenContractAddress);

            return gameAsset.BalanceOf(address);
        }

        public Task<ulong> GetAssetBalanceOf(string gameContract, PlayerID pid, ulong itemID)
        {
            GameContract game = new GameContract(web, gameContract);
            return game.GetAssetBalance(pid.ID, itemID);
        }

        public Task<ulong> GetGameAssetCount(string gameContract)
        {
            GameContract game = new GameContract(web, gameContract);
            return game.GetNextAssetIdAsync();
        }

        public async Task<GameAssetContract[]> GetGameAssetContacts(string gameContract)
        {
            GameContract game = new GameContract(web, gameContract);
            ulong length = await game.GetNextAssetIdAsync();

            GameAssetContract[] ret = new GameAssetContract[length];

            for (ulong i = 0; i < length; ++i)
            {
                var address = await game.GetGameAssetContractAsync(i);
                ret[i] = new GameAssetContract(web, address);
            }

            return ret;
        }

        public async Task<GameExchangeContract> GetGameExchangeContract(string gameContract)
        {
            GameContract game = new GameContract(web, gameContract);

            return new GameExchangeContract(web, await game.GameExchangeContractAsync());
        }

        public Task<bool> RequestPayoutPlayerReward(string gameAssetContractAddress, ulong amount, string gameContract, string from)
        {
            GameContract game = new GameContract(web, gameContract);

            return game.PayoutPlayerReward(gameAssetContractAddress, amount, from);
        }

        public Task<bool> RequestAssetTransfer(string to, string gameAssetContractAddress, ulong amount, string from)
        {
            GameAssetContract assetContract = new GameAssetContract(web, gameAssetContractAddress);

            return assetContract.Transfer(to, amount, from);
        }

        // TEST METHODS BELOW.










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
