using Hoard.BC.Contracts;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC
{
    /// <summary>
    /// Utility class for Blockchain communication.
    /// Uses Nethereum library.
    /// </summary>
    public class BCComm
    {
        private Web3 web = null;
        private GameCenterContract gameCenter = null;
        private Dictionary<GameID, GameContract> gameContracts = new Dictionary<GameID, GameContract>();

        public BCComm(Nethereum.JsonRpc.Client.IClient client, string gameCenterContract)
        {
            web = new Web3(client);

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

        public async Task<string[]> GetGameItemContracts(GameID game)
        {
            GameContract gameContract = new GameContract(web, gameContracts[game].Address);

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
            gameContracts.Clear();

            ulong count = await gameCenter.GetGameCount();
            GameID[] games = new GameID[count];
            for(ulong i =0;i<count;++i)
            {
                BigInteger gameID = await gameCenter.GetGameIdByIndexAsync(i);
                string gameAddress = await gameCenter.GetGameContractAsync(gameID);
                GameID game = new GameID(gameID.ToString("x"));                
                {
                    GameContract gameContract = new GameContract(web, gameAddress);

                    string url = await gameContract.GetGameServerURLAsync();

                    game.Name = await gameContract.GetName();
                    game.Url = !url.StartsWith("http") ? "http://" + url : url;

                    gameContracts.Add(game, gameContract);
                }

                games[i] = game;
            }
            return games;
        }

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
        public async Task<TransactionReceipt> EvaluateOnBC(PlayerID account, Function function, params object[] functionInput)
        {
            if (account == null)
            {
                // use default user
                account = HoardService.Instance.DefaultPlayer;
            }

            int unlockTime = 120;
            bool success = web.Personal.UnlockAccount.SendRequestAsync(account.ID, account.Password, unlockTime).Result;
            if (success)
            {
                HexBigInteger gas = function.EstimateGasAsync(account.ID, new HexBigInteger(300000), new HexBigInteger(0), functionInput).Result;
                return await function.SendTransactionAndWaitForReceiptAsync(account.ID, gas, new HexBigInteger(0), null, functionInput);
            }

            return null;
        }
    }
}
