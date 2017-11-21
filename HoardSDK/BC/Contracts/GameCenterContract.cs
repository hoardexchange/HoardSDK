using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Org.BouncyCastle.Math;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    class GameInfoDTO
    {
        [Parameter("uint64", "id", 1, true)]
        public ulong ID { get; set; }

        [Parameter("bytes32", "name", 2, true)]
        public byte[] Name { get; set; }

    }

    class GameCenterContract
    {        
        public static string ABI = @"[{'constant':true,'inputs':[{'name':'gameId','type':'uint64'}],'name':'getGameInfo','outputs':[{'name':'id','type':'uint64'},{'name':'name','type':'bytes32'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'gameId','type':'uint64'}],'name':'removeGame','outputs':[{'name':'','type':'bool'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'gameId','type':'uint64'},{'name':'name','type':'bytes32'},{'name':'gameOwner','type':'address'}],'name':'addGame','outputs':[{'name':'','type':'bool'}],'payable':false,'type':'function'},{'constant':true,'inputs':[{'name':'gameId','type':'uint64'}],'name':'getGameContact','outputs':[{'name':'','type':'address'}],'payable':false,'type':'function'},{'constant':true,'inputs':[{'name':'gameId','type':'uint64'}],'name':'gameExists','outputs':[{'name':'','type':'bool'}],'payable':false,'type':'function'},{'inputs':[],'payable':false,'type':'constructor'},{'anonymous':false,'inputs':[{'indexed':false,'name':'gameOwner','type':'address'},{'indexed':false,'name':'gameId','type':'uint64'}],'name':'GameAdded','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'gameOwner','type':'address'},{'indexed':false,'name':'gameId','type':'uint64'}],'name':'GameRemoved','type':'event'}]";

        private readonly Web3 web3;
        private Contract contract;

        public GameCenterContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        public Function GetFunctionGetGameInfo()
        {
            return contract.GetFunction("getGameInfo");
        }

        public Function GetFunctionGetGameContract()
        {
            return contract.GetFunction("getGameContact");
        }

        public Function GetFunctionGameExists()
        {
            return contract.GetFunction("gameExists");
        }

        public Function GetFunctionAddGame()
        {
            return contract.GetFunction("addGame");
        }

        public Task<string> GetGameContractAsync(ulong gameID)
        {
            var function = GetFunctionGetGameContract();
            return function.CallAsync<string>(gameID);
        }

        public Task<GameInfoDTO> GetGameInfoAsync(ulong gameID)
        {
            var function = GetFunctionGetGameInfo();
            return function.CallAsync<GameInfoDTO>(gameID);
        }

        public Task<bool> GetGameExistsAsync(ulong gameID)
        {
            var function = GetFunctionGameExists();
            return function.CallAsync<bool>(gameID);
        }

        public async Task<TransactionReceipt> MineAndGetReceiptAsync(Web3 web3, string transactionHash)
        {
            //await new Nethereum.Geth.RPC.Miner.MinerStart(web3.Client).SendRequestAsync(6);

            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

            while (receipt == null)
            {
                Thread.Sleep(1000);
                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }

           // await new Nethereum.Geth.RPC.Miner.MinerStop(web3.Client).SendRequestAsync();
            return receipt;
        }

        public async Task<string> AddGameAsync(ulong id, string name, string owner)
        {
            var function = GetFunctionAddGame();
            byte[] nameBin = System.Text.Encoding.UTF8.GetBytes(name);

            //
            string pw = "dev";
            string address = "0xa0464599df2154ec933497d712643429e81d4628";// Nethereum.Signer.EthECKey.GetPublicAddress(privateKey); //could do checksum
            var accountUnlockTime = 120;
            var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(address, pw, accountUnlockTime);
            Task<string> ts = function.SendTransactionAsync(address, new HexBigInteger(4700000), new HexBigInteger(0), id, name, owner);
            var txHash = await ts;
            var receipt = await MineAndGetReceiptAsync(web3, txHash);            

            return "dupa";
            //var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address).ConfigureAwait(false);

            //return false;
            /*var data = function.GetData(score);
            var encoded = web3.OfflineTransactionSigning.SignTransaction(privateKey, contract.Address, 0,
              txCount.Value, 1000000000000L, 900000, data);
            return await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(encoded).ConfigureAwait(false);

            var
            function = GetFunctionSetTopScore();
            return function.CreateTransactionInput(addressFrom, gas, valueAmount, score, ethEcdsa.V, ethEcdsa.R, ethEcdsa.S);
            //
            function.CreateTransactionInput("0xa0464599df2154ec933497d712643429e81d4628",)
            return function.SendTransactionAsync(id, name, owner);*/
        }
    }
}
