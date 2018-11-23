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
                Trace.Fail(ex.Message);
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        public async Task<HexBigInteger> GetBalance(string account)
        {
            var ver = new Nethereum.RPC.Eth.EthGetBalance(web.Client);
            return await ver.SendRequestAsync(account);
        }

        public async Task<string> GetHRDAddressAsync()
        {
            return await gameCenter.GetHoardTokenAddressAsync();
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
                return true;
            }

            string gameAddress = await gameCenter.GetGameContractAsync(game.ID);
            GameContract gameContract = new GameContract(web, gameAddress);

            string url = await gameContract.GetGameServerURLAsync();

            game.Name = await gameContract.GetName();
            game.GameOwner = await gameContract.GetOwner();
            game.Url = !url.StartsWith("http") ? "http://" + url : url;

            gameContracts.Add(game, gameContract);
            return true;
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

        public async Task<GameExchangeContract> GetGameExchangeContractAsync()
        {
            string exchangeAddress = await gameCenter.GetExchangeAddressAsync();
            if (exchangeAddress != null)
            {
                if (exchangeAddress.StartsWith("0x"))
                    exchangeAddress = exchangeAddress.Substring(2);

                BigInteger exchangeAddressInt = BigInteger.Parse(exchangeAddress, NumberStyles.AllowHexSpecifier);
                if (!exchangeAddressInt.Equals(0))
                    return new GameExchangeContract(web, exchangeAddress);
            }
            return null;
        }

        public async Task<bool> TransferHRDAsync(string from, string to, ulong amount)
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
            return false;
        }

        public async Task<TransactionReceipt> SetExchangeContractAsync(User user, string exchangeAddress)
        {
            return await gameCenter.SetExchangeAddressAsync(this, exchangeAddress, user);
        }

        public async Task<TransactionReceipt> SetHoardTokenAddressAsync(User user, string hoardTokenAddress)
        {
            return await gameCenter.SetHoardTokenAddressAsync(this, hoardTokenAddress, user);
        }

        public async Task<TransactionReceipt> SetExchangeSrvURLAsync(User user, string url)
        {
            return await gameCenter.SetExchangeSrvURLAsync(this, url, user);
        }

        public async Task<string> GetGameExchangeSrvURL()
        {
            string url = await gameCenter.GetExchangeSrvURLAsync();
            url = !url.StartsWith("http") ? "http://" + url : url;
            return url;
        }

        // TEST METHODS BELOW.







        /// <summary>
        /// temporary function to call functions on blockchain
        /// </summary>
        /// <returns></returns>
        public async Task<TransactionReceipt> EvaluateOnBC(User user, Function function, params object[] functionInput)
        {
            if (user == null)
            {
                // use default user
                user = HoardService.Instance.DefaultUser;
            }
            Debug.Assert(user.ActiveAccount != null);

            HexBigInteger gas = await function.EstimateGasAsync(user.ActiveAccount.ID, new HexBigInteger(300000), new HexBigInteger(0), functionInput);

            var nonceService = new InMemoryNonceService(user.ActiveAccount.ID, web.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();

            string data = function.GetData(functionInput);
            BigInteger gasPrice = BigInteger.Zero;
            var trans = new Nethereum.Signer.Transaction(function.ContractAddress,
                BigInteger.Zero, nonce, gasPrice, gas.Value, data);
            string encoded = user.ActiveAccount.Sign(trans.GetRLPEncodedRaw()).Result;
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
