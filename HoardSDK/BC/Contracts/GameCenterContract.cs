﻿using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;

namespace Hoard.BC.Contracts
{
    [FunctionOutput]
    class GameInfoDTO
    {
        [Parameter("uint64", "id", 1)]
        public ulong ID { get; set; }

        [Parameter("bytes32", "name", 2)]
        public string Name { get; set; }

    }

    /// <summary>
    /// Main Hoard contract with a list of all games registered on platform. Central point from which we can get all neccessery data.
    /// </summary>
    internal class GameCenterContract
    {
        public const string ABI = HoardABIConfig.HoardGameCenterABI;

        private readonly Web3 web3;
        private Contract contract;

        public string Address { get { return contract.Address; } }

        public GameCenterContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        public Function GetFunctionName()
        {
            return contract.GetFunction("name");
        }

        public Function GetFunctionGetGameContract()
        {
            return contract.GetFunction("getGameContract");
        }

        public Function GetFunctionGetGameIdByIndex()
        {
            return contract.GetFunction("getGameIdByIndex");
        }

        public Function GetFunctionGameExists()
        {
            return contract.GetFunction("gameExists");
        }

        public Function GetFunctionAddGame()
        {
            return contract.GetFunction("addGame");
        }

        public Function GetFunctionAddAdmin()
        {
            return contract.GetFunction("addAdmin");
        }

        public Function GetFunctionRemoveAdmin()
        {
            return contract.GetFunction("removeAdmin");
        }

        /// <summary>
        /// Returns game contract address by id
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public Task<string> GetGameContractAsync(BigInteger gameID)
        {
            var function = GetFunctionGetGameContract();
            return function.CallAsync<string>(gameID);
        }

        /// <summary>
        /// Returns game id address by index
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public Task<BigInteger> GetGameIdByIndexAsync(ulong index)
        {
            var function = GetFunctionGetGameIdByIndex();
            return function.CallAsync<BigInteger>(index);
        }

        /// <summary>
        /// Returns total number of registered games on Hoard Platform
        /// </summary>
        /// <returns></returns>
        public Task<ulong> GetGameCount()
        {
            Function function = contract.GetFunction("nextGameIndex");
            return function.CallAsync<ulong>();
        }

        public Task<string> NameAsync(ulong gameID)
        {
            var function = GetFunctionName();
            return function.CallAsync<string>();
        }

        public Task<bool> GetGameExistsAsync(string gameID)
        {
            var function = GetFunctionGameExists();
            return function.CallAsync<bool>(gameID);
        }

        public async Task<TransactionReceipt> AddGameAsync(BCComm comm, string gameAddr, PlayerID account = null)
        {
            var function = GetFunctionAddGame();
            return await comm.EvaluateOnBC(account, function, gameAddr);
        }

        public async Task<TransactionReceipt> AddAdminAsync(BCComm comm, string adminAddr, PlayerID account = null)
        {
            var function = GetFunctionAddAdmin();
            return await comm.EvaluateOnBC(account, function, adminAddr);
        }

        public async Task<TransactionReceipt> RemoveAdminAsync(BCComm comm, string adminAddr, PlayerID account = null)
        {
            var function = GetFunctionRemoveAdmin();
            return await comm.EvaluateOnBC(account, function, adminAddr);
        }
    }
}