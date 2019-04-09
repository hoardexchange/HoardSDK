using Hoard.BC.Contracts;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RLP;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.NonceServices;
using Nethereum.Web3;
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

        /// <summary>
        /// Creates BCComm object.
        /// </summary>
        /// <param name="client">JsonRpc client implementation</param>
        /// <param name="gameCenterContract">game center contract address</param>
        public BCComm(Nethereum.JsonRpc.Client.IClient client, string gameCenterContract)
        {
            web = new Web3(client);

            if (gameCenterContract == null)
                ErrorCallbackProvider.ReportError("Game center contract cannot be null!");

            gameCenter = GetContract<GameCenterContract>(gameCenterContract);
        }

        /// <summary>
        /// Connects to blockchain using the JsonRpc client and performs a handshake
        /// </summary>
        /// <returns>a pair of [bool result, string return infromation] received from client</returns>
        public async Task<Tuple<bool,string>> Connect()
        {            
            try
            {
                var ver = new Nethereum.RPC.Web3.Web3ClientVersion(web.Client);
                string verStr = await ver.SendRequestAsync();
                //check if game contract exists
                var code = new Nethereum.RPC.Eth.EthGetCode(web.Client);
                string codeStr = await code.SendRequestAsync(gameCenter.Address);
                return new Tuple<bool, string>(!string.IsNullOrEmpty(codeStr) && codeStr!="0x", verStr);
            }
            catch(Exception ex)
            {
                ErrorCallbackProvider.ReportError(ex.ToString());
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        /// <summary>
        /// Returns ETH balance of given account
        /// </summary>
        /// <param name="account">account to query</param>
        /// <returns></returns>
        public async Task<BigInteger> GetETHBalance(HoardID account)
        {
            var ver = new Nethereum.RPC.Eth.EthGetBalance(web.Client);
            return (await ver.SendRequestAsync(account)).Value;
        }

        /// <summary>
        /// Retrieves HRD token address from game center
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetHRDAddress()
        {
            return await gameCenter.GetHoardTokenAddressAsync();
        }

        /// <summary>
        /// Retrieves owner address from game center
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetHoardOwner()
        {
            return await gameCenter.GetContractOwner();
        }

        /// <summary>
        /// Returns HRD balance of given account
        /// </summary>
        /// <param name="account">account to query</param>
        /// <returns></returns>
        public async Task<BigInteger> GetHRDBalance(HoardID account)
        {
            string hrdAddress = await GetHRDAddress();
            if (hrdAddress != null)
            {
                hrdAddress.RemoveHexPrefix();

                HoardTokenContract hrdContract = new HoardTokenContract(web, hrdAddress);
                return await hrdContract.GetBalanceOf(account);
            }
            return new BigInteger(0);
        }

        /// <summary>
        /// Returns GameItem contract for given game and of given type
        /// </summary>
        /// <param name="game"></param>
        /// <param name="contractAddress"></param>
        /// <param name="contractType"></param>
        /// <param name="abi">[optional] creates contract with a particular abi</param>
        /// <returns></returns>
        public GameItemContract GetGameItemContract(GameID game, string contractAddress, Type contractType, string abi = "")
        {
            if (abi == "")
            {
                return (GameItemContract)Activator.CreateInstance(contractType, game, web, contractAddress);
            }
            else
            {
                return (GameItemContract)Activator.CreateInstance(contractType, game, web, contractAddress, abi);
            }
        }

        /// <summary>
        /// Helper function to get contract of a prticular type
        /// </summary>
        /// <typeparam name="TContract">type of contract</typeparam>
        /// <param name="contractAddress">address of the contract</param>
        /// <returns></returns>
        public TContract GetContract<TContract>(string contractAddress)
        {
            return (TContract)Activator.CreateInstance(typeof(TContract), web, contractAddress);
        }

        /// <summary>
        /// Retrieves all GameItem contract addresses registered for a particular game
        /// </summary>
        /// <param name="game">game to query</param>
        /// <returns></returns>
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

        /// <summary>
        /// Registers existing Hoard game. This game must exist on Hoard platform.
        /// This function performs initial setup of game contract.
        /// </summary>
        /// <param name="game">[in/out] game object must contain valid ID. Other fields will be retrieved from platform</param>
        /// <returns></returns>
        public async Task<bool> RegisterHoardGame(GameID game)
        {
            if (gameContracts.ContainsKey(game))
            {
                ErrorCallbackProvider.ReportWarning("Game already registered!");
                return true;
            }

            string gameAddress = await gameCenter.GetGameContractAsync(game.ID);

            if (gameAddress != Eth.Utils.EMPTY_ADDRESS)
            {
                GameContract gameContract = new GameContract(web, gameAddress);

                string url = await gameContract.GetGameServerURLAsync();

                game.Name = await gameContract.GetName();
                game.GameOwner = await gameContract.GetOwner();
                if (url.Length>0)
                    game.Url = !url.StartsWith("http") ? "http://" + url : url;

                gameContracts.Add(game, gameContract);
                return true;
            }
            ErrorCallbackProvider.ReportError($"Game is not registered in Hoard Game Center: game = {game.ID}!");
            return false;
        }

        /// <summary>
        /// Removes game from system. Call when you are finished with using that game
        /// </summary>
        /// <param name="game">game to unregister</param>
        public void UnregisterHoardGame(GameID game)
        {
            gameContracts.Remove(game);
        }

        /// <summary>
        /// Returns all registered games (using RegisterHoardGame)
        /// </summary>
        /// <returns></returns>
        public GameID[] GetRegisteredHoardGames()
        {
            GameID[] games = new GameID[gameContracts.Count];
            gameContracts.Keys.CopyTo(games, 0);
            return games;
        }

        /// <summary>
        /// Retrieves all Hoard games registered on the platform.
        /// </summary>
        /// <returns></returns>
        public async Task<GameID[]> GetHoardGames()
        {
            ulong count = await gameCenter.GetGameCount();
            GameID[] games = new GameID[count];
            for (ulong i = 0; i < count; ++i)
            {
                games[i] = await GetGameByIndex(i);
            }
            return games;
        }

        /// <summary>
        /// Retrieves number of Hoard games registered on the platform
        /// </summary>
        /// <returns></returns>
        public async Task<UInt64> GetHoardGameCount()
        {
            return await gameCenter.GetGameCount();
        }

        /// <summary>
        /// Checks if game is registered on Hoard Platform
        /// </summary>
        /// <param name="gameID">game ID to check</param>
        /// <returns></returns>
        public async Task<bool> GetGameExists(BigInteger gameID)
        {
            return await gameCenter.GetGameExistsAsync(gameID);
        }

        /// <summary>
        /// Returns address of Hoard exchange contract
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetHoardExchangeContractAddress()
        {
            return await gameCenter.GetExchangeAddressAsync();
        }

        /// <summary>
        /// Returns Hoard exchange contract
        /// </summary>
        /// <returns></returns>
        internal async Task<ExchangeContract> GetHoardExchangeContract()
        {
            string exchangeAddress = await gameCenter.GetExchangeAddressAsync();
            if (exchangeAddress != null)
            {
                exchangeAddress.RemoveHexPrefix();

                BigInteger exchangeAddressInt = BigInteger.Parse(exchangeAddress, NumberStyles.AllowHexSpecifier);
                if (!exchangeAddressInt.Equals(0))
                    return new ExchangeContract(web, exchangeAddress);
            }
            return null;
        }

        /// <summary>
        /// Transfer HRD amount to another account
        /// </summary>
        /// <param name="from">sender account</param>
        /// <param name="to">receiver account</param>
        /// <param name="amount">amount to send</param>
        /// <returns>true if transfer was successful, false otherwise</returns>
        public async Task<bool> TransferHRD(AccountInfo from, string to, BigInteger amount)
        {
            string hoardTokenAddress = await gameCenter.GetHoardTokenAddressAsync();
            if (hoardTokenAddress != null)
            {
                hoardTokenAddress.RemoveHexPrefix();

                BigInteger hoardTokenAddressInt = BigInteger.Parse(hoardTokenAddress, NumberStyles.AllowHexSpecifier);
                if (!hoardTokenAddressInt.Equals(0))
                {
                    HoardTokenContract hrdContract = new HoardTokenContract(web, hoardTokenAddress);
                    return await hrdContract.Transfer(from, to, amount);
                }
            }
            ErrorCallbackProvider.ReportError("Cannot get proper Hoard Token contract!");
            return false;
        }

        /// <summary>
        /// Sets exchange contract address in game center
        /// </summary>
        /// <param name="account">game center owner account</param>
        /// <param name="exchangeAddress">address of Hoard exchange contract</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> SetExchangeContract(AccountInfo account, string exchangeAddress)
        {
            return await gameCenter.SetExchangeAddressAsync(exchangeAddress, account);
        }

        /// <summary>
        /// Sets HRD token contract address in game center
        /// </summary>
        /// <param name="account">game center owner account</param>
        /// <param name="hoardTokenAddress">address of HRD token contract</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> SetHRDAddress(AccountInfo account, string hoardTokenAddress)
        {
            return await gameCenter.SetHoardTokenAddressAsync(hoardTokenAddress, account);
        }

        /// <summary>
        /// Utility to call functions on blockchain signing it by given account
        /// </summary>
        /// <returns>Receipt of called transaction</returns>
        public static async Task<TransactionReceipt> EvaluateOnBC(Web3 web, AccountInfo account, Function function, params object[] functionInput)
        {
            Debug.Assert(account != null);

            HexBigInteger gas = await function.EstimateGasAsync(account.ID, new HexBigInteger(300000), new HexBigInteger(0), functionInput);

            var nonceService = new InMemoryNonceService(account.ID, web.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();

            string data = function.GetData(functionInput);
            var defaultGasPrice = Nethereum.Signer.TransactionBase.DEFAULT_GAS_PRICE;
            var trans = new Nethereum.Signer.Transaction(function.ContractAddress, BigInteger.Zero, nonce, defaultGasPrice, gas.Value, data);
            var rlpEncodedTx = trans.GetRLPEncodedRaw();

            // FIXME? hoard sdk api is not compatible with plasma and ethereum at the same time
            // SignTransaction should return some struct or only signature
            // temp fix - encode/decode transaction data
            string signedTransactionData = await account.SignTransaction(rlpEncodedTx);
            if (signedTransactionData == null)
            {
                ErrorCallbackProvider.ReportError("Could not sign transaction!");
                return null;
            }

            string encodedFlatten = FlattenSignedTransactionData(signedTransactionData);

            string txId = await web.Eth.Transactions.SendRawTransaction.SendRequestAsync(encodedFlatten).ConfigureAwait(false);
            TransactionReceipt receipt = await web.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
            while (receipt == null)
            {
                Thread.Sleep(1000);
                receipt = await web.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
            }
            return receipt;
        }

        /// <summary>
        /// Utility to send ETH on blockchain signing it by given account
        /// </summary>
        /// <returns>Receipt of called transaction</returns>
        public static async Task<TransactionReceipt> EvaluateOnBC(Web3 web, AccountInfo account, string to, HexBigInteger amount)
        {
            Debug.Assert(account != null);

            var nonceService = new InMemoryNonceService(account.ID, web.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();

            var trans = new Nethereum.Signer.Transaction(to, amount, nonce);
            string signedTransactionData = await account.SignTransaction(trans.GetRLPEncodedRaw());
            if (signedTransactionData == null)
            {
                ErrorCallbackProvider.ReportError("Could not sign transaction!");
                return null;
            }

            string encodedFlatten = FlattenSignedTransactionData(signedTransactionData);

            string txId = await web.Eth.Transactions.SendRawTransaction.SendRequestAsync(encodedFlatten);
            TransactionReceipt receipt = await web.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
            while (receipt == null)
            {
                Thread.Sleep(1000);
                receipt = await web.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
            }
            return receipt;
        }

        private async Task<GameID> GetGameByIndex(ulong gameIndex)
        {
            BigInteger gameID = (await gameCenter.GetGameIdByIndexAsync(gameIndex));
            string gameAddress = await gameCenter.GetGameContractAsync(gameID);
            GameID game = new GameID(gameID);
            GameContract gameContract = new GameContract(web, gameAddress);
            string url = await gameContract.GetGameServerURLAsync();
            game.Symbol = await gameContract.GetSymbol();
            game.Name = await gameContract.GetName();
            game.GameOwner = await gameContract.GetOwner();
            game.Url = !url.StartsWith("http") ? "http://" + url : url;
            return game;
        }

        static private string FlattenSignedTransactionData(string encodedTransactionData)
        {
            var encodedFlattenList = new List<byte[]>();
            RLPCollection rlpDecoded = RLP.Decode(encodedTransactionData.HexToByteArray());
            if (rlpDecoded.Count == 1)
            {
                rlpDecoded = (RLPCollection)rlpDecoded[0];
                foreach (var rlpItem in (RLPCollection)rlpDecoded[0])
                {
                    if (rlpItem.RLPData != null)
                    {
                        encodedFlattenList.Add(RLP.EncodeElement(rlpItem.RLPData));
                    }
                    else
                    {
                        encodedFlattenList.Add(RLP.EncodeElement(0.ToBytesForRLPEncoding()));
                    }
                }

                for (int i = 1; i < rlpDecoded.Count; ++i)
                {
                    encodedFlattenList.Add(RLP.EncodeElement(rlpDecoded[i].RLPData));
                }
            }

            return RLP.EncodeList(encodedFlattenList.ToArray()).ToHex().EnsureHexPrefix();
        }
    }
}
