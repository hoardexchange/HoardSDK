using PlasmaCore.RPC;
using PlasmaCore.RPC.OutputData;
using PlasmaCore.UTXO;
using System.Numerics;
using System.Threading.Tasks;

namespace PlasmaCore
{
    /// <summary>
    /// Service class for Plasma communication
    /// </summary>
    public class PlasmaAPIService
    {
        private IClient watcherClient = null;

        private IClient childChainClient = null;

        /// <summary>
        /// Creates Plasma communication service
        /// </summary>
        /// <param name="_watcherClient">plasma watcher client</param>
        /// <param name="_childChainClient">plasma child chain client (optional, currently not used)</param>
        public PlasmaAPIService(IClient _watcherClient, IClient _childChainClient = null)
        {
            watcherClient = _watcherClient;
            childChainClient = _childChainClient;
        }

        /// <summary>
        /// Returns balance data of given address
        /// </summary>
        /// <param name="address">account address</param>
        /// <returns></returns>
        public async Task<BalanceData[]> GetBalance(string address)
        {
            var getBalance = new RPC.Account.GetBalance(watcherClient);
            return await getBalance.SendRequestAsync(address);
        }

        /// <summary>
        /// Returns utxo data of given address
        /// </summary>
        /// <param name="address">account address</param>
        /// <returns></returns>
        public async Task<UTXOData[]> GetUtxos(string address)
        {
            var getUtxos = new RPC.Account.GetUtxos(watcherClient);
            return await getUtxos.SendRequestAsync(address);
        }

        /// <summary>
        /// Returns transactions data of given address
        /// </summary>
        /// <param name="address">account address</param>
        /// <param name="limit">result limit</param>
        /// <returns></returns>
        public async Task<TransactionsData[]> GetTransactions(string address, uint limit = 100)
        {
            var getTransactions = new RPC.Account.GetTransactions(watcherClient);
            return await getTransactions.SendRequestAsync(address, limit);
        }

        /// <summary>
        /// Returns challenge data for given utxo exit
        /// </summary>
        /// <param name="position">utxo position</param>
        /// <returns></returns>
        public async Task<ChallengeData> GetChallengeData(BigInteger position)
        {
            var getChallengeData = new RPC.UTXO.GetChallangeData(watcherClient);
            return await getChallengeData.SendRequestAsync(position);
        }

        /// <summary>
        /// Returns exit data for a given utxo
        /// </summary>
        /// <param name="position">utxo position</param>
        /// <returns></returns>
        public async Task<ExitData> GetExitData(BigInteger position)
        {
            var getExitData = new RPC.UTXO.GetExitData(watcherClient);
            return await getExitData.SendRequestAsync(position);
        }

        /// <summary>
        /// Returns exit data for an in-flight exit
        /// </summary>
        /// <param name="txBytes">in-flight transaction bytes body</param>
        /// <returns></returns>
        public async Task<InFlightExitData> GetInFlightExitData(string txBytes)
        {
            var getData = new RPC.InFlightExit.GetInFlightExit(watcherClient);
            return await getData.SendRequestAsync(txBytes);
        }

        /// <summary>
        /// Returns a competitor to an in-flight exit
        /// </summary>
        /// <param name="txBytes">in-flight transaction bytes body</param>
        /// <returns></returns>
        public async Task<CompetitorData> GetInFlightExitCompetitor(string txBytes)
        {
            var getCompetitor = new RPC.InFlightExit.GetCompetitor(watcherClient);
            return await getCompetitor.SendRequestAsync(txBytes);
        }

        /// <summary>
        /// Returns a proof that transaction is canonical
        /// </summary>
        /// <param name="txBytes">in-flight transaction bytes body</param>
        /// <returns></returns>
        public async Task<CanonicalProofData> ProveCanonical(string txBytes)
        {
            var proveCanonical = new RPC.InFlightExit.ProveCanonical(watcherClient);
            return await proveCanonical.SendRequestAsync(txBytes);
        }

        /// <summary>
        /// Returns transaction data with given hash
        /// </summary>
        /// <param name="txHash">transaction hash (id)</param>
        /// <returns></returns>
        public async Task<TransactionDetails> GetTransaction(string txHash)
        {
            var getTransaction = new RPC.Transaction.GetTransaction(watcherClient);
            return await getTransaction.SendRequestAsync(txHash);
        }

        /// <summary>
        /// Submits signed transaction to the child chain and returns transaction receipt
        /// </summary>
        /// <param name="transaction">signed transaction</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> SubmitTransaction(string transaction)
        {
            var submitTransaction = new RPC.Transaction.SubmitTransaction(watcherClient);
            return await submitTransaction.SendRequestAsync(transaction);
        }
    }
}
