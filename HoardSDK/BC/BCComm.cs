using Hoard.BC.Contracts;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.NonceServices;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task<Tuple<bool,string>> Connect()
        {
            var ver = new Nethereum.RPC.Web3.Web3ClientVersion(web.Client);
            try
            {
                return new Tuple<bool, string>(true, await ver.SendRequestAsync());
            }
            catch(Exception ex)
            {
                Trace.Fail(ex.ToString());
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        public async Task<HexBigInteger> GetBalance(string account)
        {
            var ver = new Nethereum.RPC.Eth.EthGetBalance(web.Client);
            return await ver.SendRequestAsync(account);
        }

        public Task<string> GetHRDAddressAsync()
        {
            return gameCenter.GetHoardTokenAddressAsync();
        }

        public async Task<BigInteger> GetHRDAmountAsync(string account)
        {
            string hrdAddress = await GetHRDAddressAsync();
            if (hrdAddress != null)
            {
                if (hrdAddress.StartsWith("0x"))
                    hrdAddress = hrdAddress.Substring(2);

                HoardTokenContract hrdContract = new HoardTokenContract(web, hrdAddress);
                return await hrdContract.GetBalanceOf(account);
            }
            return new BigInteger(0);
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

        public async Task<bool> RegisterHoardGame(GameID game)
        {
            if (gameContracts.ContainsKey(game))
            {
                Trace.TraceWarning("Game already registered!");
                return true;
            }

            string gameAddress = await gameCenter.GetGameContractAsync(game.ID);

            if (gameAddress != Eth.Utils.EMPTY_ADDRESS)
            {
                GameContract gameContract = new GameContract(web, gameAddress);

                string url = await gameContract.GetGameServerURLAsync();

                game.Name = await gameContract.GetName();
                game.GameOwner = await gameContract.GetOwner();
                game.Url = !url.StartsWith("http") ? "http://" + url : url;

                gameContracts.Add(game, gameContract);
                return true;
            }
            Trace.TraceError($"Game is not registered in Hoard Game Center: game = {game.ID}!");
            return false;
        }

        public void UnregisterHoardGame(GameID game)
        {
            gameContracts.Remove(game);
        }

        public GameID[] GetRegisteredHoardGames()
        {
            GameID[] games = new GameID[gameContracts.Count];
            gameContracts.Keys.CopyTo(games, 0);
            return games;
        }

        public async Task<GameID[]> GetHoardGames()
        {
            ulong count = await gameCenter.GetGameCount();
            GameID[] games = new GameID[count];
            for (ulong i = 0; i < count; ++i)
            {
                string gameID = (await gameCenter.GetGameIdByIndexAsync(i)).ToString("x");
                string gameAddress = await gameCenter.GetGameContractAsync(gameID);
                GameID game = new GameID(gameID);                
                GameContract gameContract = new GameContract(web, gameAddress);
                string url = await gameContract.GetGameServerURLAsync();
                game.Name = await gameContract.GetName();
                    game.GameOwner = await gameContract.GetOwner();
                game.Url = !url.StartsWith("http") ? "http://" + url : url;
                games[i] = game;
            }
            return games;
        }

        public async Task<bool> GetGameExistsAsync(string gameID)
        {
            return await gameCenter.GetGameExistsAsync(gameID);
        }

        public async Task<string> GetGameExchangeContractAddressAsync()
        {
            return await gameCenter.GetExchangeAddressAsync();
        }

        public async Task<string> GetHoardTokenAddressAsync()
        {
            return await gameCenter.GetHoardTokenAddressAsync();
        }

        public async Task<ExchangeContract> GetGameExchangeContractAsync()
        {
            string exchangeAddress = await gameCenter.GetExchangeAddressAsync();
            if (exchangeAddress != null)
            {
                if (exchangeAddress.StartsWith("0x"))
                    exchangeAddress = exchangeAddress.Substring(2);

                BigInteger exchangeAddressInt = BigInteger.Parse(exchangeAddress, NumberStyles.AllowHexSpecifier);
                if (!exchangeAddressInt.Equals(0))
                    return new ExchangeContract(web, exchangeAddress);
            }
            return null;
        }

        public async Task<bool> TransferHRDAsync(AccountInfo from, string to, BigInteger amount)
        {
            string hoardTokenAddress = await gameCenter.GetHoardTokenAddressAsync();
            if (hoardTokenAddress != null)
            {
                if (hoardTokenAddress.StartsWith("0x"))
                    hoardTokenAddress = hoardTokenAddress.Substring(2);

                BigInteger hoardTokenAddressInt = BigInteger.Parse(hoardTokenAddress, NumberStyles.AllowHexSpecifier);
                if (!hoardTokenAddressInt.Equals(0))
                {
                    HoardTokenContract hrdContract = new HoardTokenContract(web, hoardTokenAddress);
                    return await hrdContract.Transfer(from, to, amount);
                }
            }
            Trace.TraceError("Cannot get proper Hoard Token contract!");
            return false;
        }

        public async Task<TransactionReceipt> SetExchangeContractAsync(AccountInfo account, string exchangeAddress)
        {
            return await gameCenter.SetExchangeAddressAsync(exchangeAddress, account);
        }

        public async Task<TransactionReceipt> SetHoardTokenAddressAsync(AccountInfo account, string hoardTokenAddress)
        {
            return await gameCenter.SetHoardTokenAddressAsync(hoardTokenAddress, account);
        }

        // TEST METHODS BELOW.







        /// <summary>
        /// temporary function to call functions on blockchain
        /// </summary>
        /// <returns></returns>
        public static async Task<TransactionReceipt> EvaluateOnBC(Web3 web, AccountInfo account, Function function, params object[] functionInput)
        {
            Debug.Assert(account != null);

            HexBigInteger gas = await function.EstimateGasAsync(account.ID, new HexBigInteger(300000), new HexBigInteger(0), functionInput);

            var nonceService = new InMemoryNonceService(account.ID, web.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();

            string data = function.GetData(functionInput);
            var trans = new Nethereum.Signer.Transaction(function.ContractAddress, BigInteger.Zero, nonce, BigInteger.Zero, gas.Value, data);
            string encoded = account.SignTransaction(trans.GetRLPEncodedRaw()).Result;
            if (encoded == null)
            {
                Trace.Fail("Could not sign transaction!");
                return null;
            }

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
