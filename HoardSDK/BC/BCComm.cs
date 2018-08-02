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

        public BCComm(Nethereum.JsonRpc.Client.IClient client, PlayerID account, string gameCenterContract)
        {
            web = new Web3(new Account(account.PrivateKey), client);

            gameCenter = GetContract<GameCenterContract>(gameCenterContract);
        }

        public async Task<string> Connect()
        {
            var ver = new Nethereum.RPC.Web3.Web3ClientVersion(web.Client);
            return await ver.SendRequestAsync();
        }

        public GameItemContract GetGameItemContract(string contractAddress, Type contractType)
        {
            return (GameItemContract)Activator.CreateInstance(contractType, web, contractAddress);
        }

        public TContract GetContract<TContract>(string contractAddress)
        {
            return (TContract)Activator.CreateInstance(typeof(TContract), web, contractAddress);
        }

        public async Task<GameID> GetGameID(string gameID)
        {
            bool exist = await gameCenter.GetGameExistsAsync(gameID);
            if (exist)
            {
                GameID game = new GameID(gameID);
                {
                    //gameID is the address of contract
                    GameContract gameContract = new GameContract(web, gameID);

                    string url = await gameContract.GetGameServerURLAsync();

                    game.Name = await gameContract.Name();
                    game.Url = !url.StartsWith("http") ? "http://" + url : url;
                }

                return game;
            }
            return null;
        }

        public async Task<string[]> GetGameItemContracts(GameID game)
        {
            GameContract gameContract = new GameContract(web, game.ID);

            ulong count = await gameContract.GetGameItemContractCountAsync();            

            string[] contracts = new string[count];
            for (ulong i = 0; i < count; ++i)
            {
                contracts[i] = await gameContract.GetGameItemContractAsync(i);
            }

            return contracts;
        }

        public async Task<GameID[]> GetHoardGames()
        {
            ulong count = await gameCenter.GetGameCount();
            GameID[] games = new GameID[count];
            for(ulong i=0;i<count;++i)
            {
                string gameID = await gameCenter.GetGameContractAsync(i);
                GameID game = new GameID(gameID);                
                {
                    //gameID is the address of contract
                    GameContract gameContract = new GameContract(web, gameID);

                    string url = await gameContract.GetGameServerURLAsync();

                    game.Name = await gameContract.Name();
                    game.Url = !url.StartsWith("http") ? "http://" + url : url;
                }

                games[i] = game;
            }
            return games;
        }

        // FIXME: Do we need this? How can we distinguish correct abi ERC20/ERC721/others?
        //public async Task<GameAssetContract[]> GetGameAssetContacts(string gameContract)
        //{
        //    GameContract game = new GameContract(web, gameContract);
        //    ulong length = await game.GetNextAssetIdAsync();

        //    GameAssetContract[] ret = new GameAssetContract[length];

        //    for (ulong i = 0; i < length; ++i)
        //    {
        //        var address = await game.GetGameItemContractAsync(i);
        //        ret[i] = new GameAssetContract(web, address);
        //    }

        //    return ret;
        //}

        public async Task<GameExchangeContract> GetGameExchangeContract(string gameContract)
        {
            GameContract game = new GameContract(web, gameContract);

            return new GameExchangeContract(web, await game.GameExchangeContractAsync());
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
