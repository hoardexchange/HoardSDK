using PlasmaCore.RPC;
using PlasmaCore.RPC.OutputData;
using PlasmaCore.UTXO;
using System.Numerics;
using System.Threading.Tasks;

namespace PlasmaCore
{
    public class PlasmaAPIService
    {
        private IClient childChainClient = null;
        private IClient watcherClient = null;

        public PlasmaAPIService(IClient _childChainClient, IClient _watcherClient)
        {
            childChainClient = _childChainClient;
            watcherClient = _watcherClient;
        }

        public async Task<BalanceData[]> GetBalance(string address)
        {
            var getBalance = new RPC.Account.GetBalance(watcherClient);
            return await getBalance.SendRequestAsync(address);
        }
        
        public async Task<UTXOData[]> GetUtxos(string address)
        {
            var getUtxos = new RPC.Account.GetUtxos(watcherClient);
            return await getUtxos.SendRequestAsync(address);
        }

        public async Task<TransactionsData[]> GetTransactions(string address, uint limit = 100)
        {
            var getTransactions = new RPC.Account.GetTransactions(watcherClient);
            return await getTransactions.SendRequestAsync(address, limit);
        }

        public async Task<ChallengeData> GetChallengeData(BigInteger position)
        {
            var getChallengeData = new RPC.UTXO.GetChallangeData(watcherClient);
            return await getChallengeData.SendRequestAsync(position);
        }

        public async Task<ExitData> GetExitData(BigInteger position)
        {
            var getExitData = new RPC.UTXO.GetExitData(watcherClient);
            return await getExitData.SendRequestAsync(position);
        }

        public async Task<InFlightExitData> GetInFlightExitData(string txBytes)
        {
            var getData = new RPC.InFlightExit.GetData(watcherClient);
            return await getData.SendRequestAsync(txBytes);
        }

        public async Task<TransactionDetails> GetTransaction(string txHash)
        {
            var getTransaction = new RPC.Transaction.GetTransaction(watcherClient);
            return await getTransaction.SendRequestAsync(txHash);
        }

        public async Task<TransactionReceipt> SubmitTransaction(string transaction)
        {
            var submitTransaction = new RPC.Transaction.SubmitTransaction(childChainClient);
            return await submitTransaction.SendRequestAsync(transaction);
        }
    }
}
