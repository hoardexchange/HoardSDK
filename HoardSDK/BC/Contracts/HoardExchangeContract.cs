using Nethereum.Contracts;
using Nethereum.RPC.NonceServices;
using Nethereum.Web3;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    /// <summary>
    /// Hoard exchange contract used for order management and making trades
    /// </summary>
    public class ExchangeContract
    { 
        private const string ABI = HoardABIConfig.HoardExchangeABI;

        private readonly Web3 web3;
        private Contract contract;

        /// <summary>
        /// Address of this contract
        /// </summary>
        public string Address { get { return contract.Address; } }

        /// <summary>
        /// Creates new ExchnageContract instance
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">eth address of this contract</param>
        public ExchangeContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        private Function GetFunctionTrade()
        {
            return contract.GetFunction("trade");
        }

        private Function GetFunctionOrder()
        {
            return contract.GetFunction("order");
        }

        private Function GetFunctionTradeERC721()
        {
            return contract.GetFunction("tradeERC721");
        }

        private Function GetFunctionOrderERC721()
        {
            return contract.GetFunction("orderERC721");
        }

        private Function GetFunctionTestTrade()
        {
            return contract.GetFunction("testTrade");
        }

        private Function GetFunctionTestTradeERC721()
        {
            return contract.GetFunction("testTradeERC721");
        }

        private Function GetFunctionWithdrawToken()
        {
            return contract.GetFunction("withdrawToken");
        }

        private Function GetFunctionWithdrawTokenERC721()
        {
            return contract.GetFunction("withdrawTokenERC721");
        }

        private Function GetFunctionCancelOrder()
        {
            return contract.GetFunction("cancelOrder");
        }

        private Function GetFunctionCancelOrderERC721()
        {
            return contract.GetFunction("cancelOrderERC721");
        }

        /// <summary>
        /// Creates a trade order for an ERC223 tokens in currency expressed in ERC233 tokens
        /// </summary>
        /// <param name="from">seller account</param>
        /// <param name="tokenGet">type of currency</param>
        /// <param name="amountGet">price in given currency</param>
        /// <param name="tokenGive">type of token to sell</param>
        /// <param name="amountGive">amount of tokens to sell</param>
        /// <param name="blockTimeDuration">expiration time of order in block number</param>
        /// <returns>true if order has been created, false otherwise</returns>
        public async Task<bool> Order(
            AccountInfo from,
            string tokenGet,
            BigInteger amountGet,
            string tokenGive,
            BigInteger amountGive,
            ulong blockTimeDuration)
        {
            var nonceService = new InMemoryNonceService(from.ID, web3.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();
            var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var expires = blockNumber.Value + blockTimeDuration;

            var function = GetFunctionOrder();
            var receipt = await BCComm.EvaluateOnBC(web3, from, function, tokenGet, amountGet, tokenGive, amountGive, expires, nonce);
            return receipt.Status.Value == 1;
        }

        /// <summary>
        /// Creates a trade order for an ERC721 tokens in currency expressed in ERC233 tokens
        /// </summary>
        /// <param name="from">seller account</param>
        /// <param name="tokenGet">currency type</param>
        /// <param name="amountGet">price in currency</param>
        /// <param name="tokenGive">type of token to sell</param>
        /// <param name="tokenId">identifier of given token to sell (which item to sell)</param>
        /// <param name="blockTimeDuration">expiration time of order in block number</param>
        /// <returns>true if order has been created, false otherwise</returns>
        public async Task<bool> OrderERC721(
            AccountInfo from,
            string tokenGet,
            BigInteger amountGet,
            string tokenGive,
            BigInteger tokenId,
            ulong blockTimeDuration)
        {
            var nonceService =  new InMemoryNonceService(from.ID, web3.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();
            var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var expires = blockNumber.Value + blockTimeDuration;

            var function = GetFunctionOrderERC721();
            var receipt = await BCComm.EvaluateOnBC(web3, from, function, tokenGet, amountGet, tokenGive, tokenId, expires, nonce);
            return receipt.Status.Value == 1;
        }

        /// <summary>
        /// Consumes an order and 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="tokenGet"></param>
        /// <param name="amountGet"></param>
        /// <param name="tokenGive"></param>
        /// <param name="amountGive"></param>
        /// <param name="expires"></param>
        /// <param name="nonce"></param>
        /// <param name="orderOwner"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<bool> Trade(
            AccountInfo from,
            string tokenGet,
            BigInteger amountGet, 
            string tokenGive,
            BigInteger amountGive,
            BigInteger expires,
            BigInteger nonce,
            string orderOwner,
            BigInteger amount)
        {
            var testTradeFun = GetFunctionTestTrade();

            var test = await testTradeFun.CallAsync<bool>(
                tokenGet,
                amountGet,
                tokenGive,
                amountGive,
                expires,
                nonce,
                orderOwner,
                amount,
                from.ID);

            if (!test)
                return false;

            var function = GetFunctionTrade();
            var receipt = await BCComm.EvaluateOnBC(web3, from, function, tokenGet, amountGet, tokenGive, amountGive, expires, nonce, orderOwner, amount);
            return receipt.Status.Value == 1;
        }

        public async Task<bool> TradeERC721(
            AccountInfo from,
            string tokenGet,
            BigInteger amountGet,
            string tokenGive,
            BigInteger tokenId,
            BigInteger expires,
            BigInteger nonce,
            string orderOwner,
            BigInteger amount)
        {
            var testTradeFun = GetFunctionTestTradeERC721();

            var test = await testTradeFun.CallAsync<bool>(
                tokenGet,
                amountGet,
                tokenGive,
                tokenId,
                expires,
                nonce,
                orderOwner,
                amount,
                from.ID);

            if (!test)
                return false;

            var function = GetFunctionTradeERC721();
            var receipt = await BCComm.EvaluateOnBC(web3, from, function, tokenGet, amountGet, tokenGive, tokenId, expires, nonce, orderOwner, amount);
            return receipt.Status.Value == 1;
        }

        public async Task<bool> Withdraw(AccountInfo from, string tokenAddress, BigInteger value)
        {
            var function = GetFunctionWithdrawToken();
            var receipt = await BCComm.EvaluateOnBC(web3, from, function, tokenAddress, value);
            return receipt.Status.Value == 1;
        }

        public async Task<bool> WithdrawERC721(AccountInfo from, string tokenAddress, BigInteger tokenId)
        {
            var function = GetFunctionWithdrawTokenERC721();
            var receipt = await BCComm.EvaluateOnBC(web3, from, function, tokenAddress, tokenId);
            return receipt.Status.Value == 1;
        }

        public async Task<bool> CancelOrder(
            AccountInfo from,
            string tokenGet,
            BigInteger amountGet,
            string tokenGive,
            BigInteger amountGive,
            BigInteger expires,
            BigInteger nonce)
        {
            var function = GetFunctionCancelOrder();
            var receipt = await BCComm.EvaluateOnBC(web3, from, function, tokenGet, amountGet, tokenGive, amountGive, expires, nonce);
            return receipt.Status.Value == 1;
        }

        public async Task<bool> CancelOrderERC721(
            AccountInfo from,
            string tokenGet,
            BigInteger amountGet,
            string tokenGive,
            BigInteger tokenIdGive,
            BigInteger expires,
            BigInteger nonce)
        {
            var function = GetFunctionCancelOrderERC721();
            var receipt = await BCComm.EvaluateOnBC(web3, from, function, tokenGet, amountGet, tokenGive, tokenIdGive, expires, nonce);
            return receipt.Status.Value == 1;
        }
    }
}
