using Hoard.BC.Contracts;
using Hoard.Interfaces;
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
    public class BCComm : IBCComm
    {
        private Web3 web = null;
        private GameCenterContract gameCenter = null;
        private Dictionary<GameID, GameContract> gameContracts = new Dictionary<GameID, GameContract>();

        private static string HRDAddress = null;

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

            gameCenter = (GameCenterContract)GetContract(typeof(GameCenterContract), gameCenterContract);
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
                bool result = !string.IsNullOrEmpty(codeStr) && codeStr != "0x";
                if (!result)
                    ErrorCallbackProvider.ReportError("Could not find Game Center contract at address: " + gameCenter.Address);
                return new Tuple<bool, string>(result, verStr);
            }
            catch(Exception ex)
            {
                ErrorCallbackProvider.ReportError(ex.ToString());
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<BigInteger> GetBalance(HoardID account)
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
            if (HRDAddress == null)
            {
                HRDAddress = await gameCenter.GetHoardTokenAddressAsync();
            }
            return HRDAddress;
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
                return (GameItemContract)Activator.CreateInstance(contractType, new object[] { game, web, contractAddress });
            }
            else
            {
                return (GameItemContract)Activator.CreateInstance(contractType, new object[] { game, web, contractAddress, abi });
            }
        }

        /// <summary>
        /// Helper function to get contract of a prticular type
        /// </summary>
        /// <param name="contractType">type of contract</param>
        /// <param name="contractAddress">address of the contract</param>
        /// <returns></returns>
        public object GetContract(Type contractType, string contractAddress)
        {
            if (contractType == typeof(GameCenterContract))
                return new GameCenterContract(web, contractAddress);
            else if (contractType == typeof(SupportsInterfaceWithLookupContract))
                return new SupportsInterfaceWithLookupContract(web, contractAddress);

            ErrorCallbackProvider.ReportError($"Unknown contract type: {contractType.ToString()}");
            return null;
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
        public async Task<Result> RegisterHoardGame(GameID game)
        {
            if (gameContracts.ContainsKey(game))
            {
                ErrorCallbackProvider.ReportWarning("Game already registered!");
                return Result.Ok;
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
                return Result.Ok;
            }
            ErrorCallbackProvider.ReportError($"Game is not registered in Hoard Game Center: game = {game.ID}!");
            return Result.GameNotFoundError;
        }

        /// <inheritdoc/>
        public void UnregisterHoardGame(GameID game)
        {
            gameContracts.Remove(game);
        }

        /// <inheritdoc/>
        public GameID[] GetRegisteredHoardGames()
        {
            GameID[] games = new GameID[gameContracts.Count];
            gameContracts.Keys.CopyTo(games, 0);
            return games;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<ulong> GetHoardGameCount()
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

        /// <inheritdoc/>
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
        /// <param name="from">sender profile</param>
        /// <param name="to">receiver address</param>
        /// <param name="amount">amount to send</param>
        /// <returns>true if transfer was successful, false otherwise</returns>
        public async Task<bool> TransferHRD(Profile from, string to, BigInteger amount)
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
        /// Transfer HRD amount to another account
        /// </summary>
        /// <param name="from">sender profile</param>
        /// <param name="to">receiver address</param>
        /// <param name="amount">amount to send</param>
        /// <returns>true if transfer was successful, false otherwise</returns>
        public async Task<bool> TransferETH(Profile from, string to, BigInteger amount)
        {
            var receipt = await EvaluateOnBC(web, from, to, new HexBigInteger(Nethereum.Util.UnitConversion.Convert.ToWei(amount)));
            return receipt.Status.Value == 1;
        }

        /// <summary>
        /// Sets exchange contract address in game center
        /// </summary>
        /// <param name="profile">game center owner profile</param>
        /// <param name="exchangeAddress">address of Hoard exchange contract</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> SetExchangeContract(Profile profile, string exchangeAddress)
        {
            return await gameCenter.SetExchangeAddressAsync(exchangeAddress, profile);
        }

        /// <summary>
        /// Sets HRD token contract address in game center
        /// </summary>
        /// <param name="profile">game center owner profile</param>
        /// <param name="hoardTokenAddress">address of HRD token contract</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> SetHRDAddress(Profile profile, string hoardTokenAddress)
        {
            return await gameCenter.SetHoardTokenAddressAsync(hoardTokenAddress, profile);
        }

        /// <summary>
        /// Utility to call functions on blockchain signing it by given account
        /// </summary>
        /// <returns>Receipt of called transaction</returns>
        public static async Task<TransactionReceipt> EvaluateOnBC(Web3 web, Profile profile, Function function, params object[] functionInput)
        {
            Debug.Assert(profile != null);

            HexBigInteger gas = await function.EstimateGasAsync(profile.ID, new HexBigInteger(300000), new HexBigInteger(0), functionInput);

            var nonceService = new InMemoryNonceService(profile.ID, web.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();

            string data = function.GetData(functionInput);
            var defaultGasPrice = Nethereum.Signer.TransactionBase.DEFAULT_GAS_PRICE;
            var transaction = new Nethereum.Signer.Transaction(function.ContractAddress, BigInteger.Zero, nonce, defaultGasPrice, gas.Value, data);
            var rlpEncodedTx = transaction.GetRLPEncodedRaw();
            
            string signature = await profile.SignTransaction(rlpEncodedTx);
            transaction.SetSignature(Nethereum.Signer.EthECDSASignatureFactory.ExtractECDSASignature(signature));

            var signedTransaction = transaction.GetRLPEncoded().ToHex(true);
            string txId = await web.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction).ConfigureAwait(false);
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
        public static async Task<TransactionReceipt> EvaluateOnBC(Web3 web, Profile profile, string to, HexBigInteger amount)
        {
            Debug.Assert(profile != null);

            var nonceService = new InMemoryNonceService(profile.ID, web.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();

            var transaction = new Nethereum.Signer.Transaction(to, amount, nonce);
            var rlpEncodedTx = transaction.GetRLPEncodedRaw();
            string signature = await profile.SignTransaction(rlpEncodedTx);
            transaction.SetSignature(Nethereum.Signer.EthECDSASignatureFactory.ExtractECDSASignature(signature));

            var signedTransaction = transaction.GetRLPEncoded().ToHex(true);
            string txId = await web.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);
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
            game.Url = ((url != null) && (!url.StartsWith("http"))) ? "http://" + url : url;
            return game;
        }
    }
}
