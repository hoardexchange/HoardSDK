using Hoard.BC.Contracts;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.NonceServices;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading;
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

        public GameItemContract GetGameItemContract(GameID game, string contractAddress, Type contractType)
        {
            return (GameItemContract)Activator.CreateInstance(contractType, game, web, contractAddress);
        }

        public TContract GetContract<TContract>(string contractAddress)
        {
            return (TContract)Activator.CreateInstance(typeof(TContract), web, contractAddress);
        }

        public async Task<string[]> GetGameItemContracts(GameID game)
        {
            if (gameContracts.ContainsKey(game))
            {
                GameContract gameContract = new GameContract(web, gameContracts[game].Address);

                ulong count = await gameContract.GetGameItemContractCountAsync();

                string[] contracts = new string[count];
                for (ulong i = 0; i < count; ++i)
                {
                    BigInteger gameId = await gameContract.GetGameItemIdByIndexAsync(i);
                    contracts[i] = await gameContract.GetGameItemContractAsync(gameId);
                }

                return contracts;
            }

            return null;
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

        public async Task<GameExchangeContract> GetGameExchangeContract(GameID game)
        {
            GameContract gameContract = null;
            gameContracts.TryGetValue(game, out gameContract);

            string exchangeAddress = await gameContract.GameExchangeContractAsync();
            if (exchangeAddress.StartsWith("0x"))
                exchangeAddress = exchangeAddress.Substring(2);

            BigInteger exchangeAddressInt = BigInteger.Parse(exchangeAddress, NumberStyles.AllowHexSpecifier);
            if (!exchangeAddressInt.Equals(0))
                return new GameExchangeContract(web, await gameContract.GameExchangeContractAsync());
            else
                return null;
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

            HexBigInteger gas = await function.EstimateGasAsync(account.ID, new HexBigInteger(300000), new HexBigInteger(0), functionInput);

            Account acc = new Account(account.PrivateKey);
            if (acc.NonceService == null)
            {
                acc.NonceService = new InMemoryNonceService(acc.Address, web.Client);
            }
            acc.NonceService.Client = web.Client;
            BigInteger nonce = await acc.NonceService.GetNextNonceAsync();

            string data = function.GetData(functionInput);
            BigInteger gasPrice = BigInteger.Zero;
            string encoded = Web3.OfflineTransactionSigner.SignTransaction(account.PrivateKey, function.ContractAddress, 
                BigInteger.Zero, nonce, gasPrice, gas.Value, data);

            //bool success = Web3.OfflineTransactionSigner.VerifyTransaction(encoded);

            string txId = await web.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encoded);
            TransactionReceipt receipt = await web.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
            while (receipt == null)
            {
                Thread.Sleep(1000);
                receipt = await web.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
            }
            return receipt;
        }
    }
}
