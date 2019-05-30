using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.NonceServices;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.Web3;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Plasma.RootChain.Contracts
{
    public static class ContractHelper
    {
        public static async Task<Nethereum.Signer.Transaction> CreateTransaction(Web3 web, string addressFrom, BigInteger amount, Function function, params object[] functionInput)
        {
            HexBigInteger gas = await function.EstimateGasAsync(addressFrom, new HexBigInteger(300000), new HexBigInteger(0), functionInput);

            var nonceService = new InMemoryNonceService(addressFrom, web.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();

            string data = function.GetData(functionInput);
            var defaultGasPrice = Nethereum.Signer.TransactionBase.DEFAULT_GAS_PRICE;
            var transaction = new Nethereum.Signer.Transaction(function.ContractAddress, amount, nonce, defaultGasPrice, gas.Value, data);

            return transaction;
        }

        public static async Task<Nethereum.Signer.Transaction> CreateTransaction<TFunctionInput>(Web3 web, string addressFrom, BigInteger amount, Function<TFunctionInput> function, TFunctionInput functionInput)
        {
            HexBigInteger gas = await function.EstimateGasAsync(functionInput, addressFrom, new HexBigInteger(300000), new HexBigInteger(0));

            var nonceService = new InMemoryNonceService(addressFrom, web.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();

            string data = function.GetData(functionInput);
            var defaultGasPrice = Nethereum.Signer.TransactionBase.DEFAULT_GAS_PRICE;
            var transaction = new Nethereum.Signer.Transaction(function.ContractAddress, amount, nonce, defaultGasPrice, gas.Value, data);

            return transaction;
        }

        public static async Task<Nethereum.Signer.Transaction> CreateTransaction(Web3 web, string address, Function function, params object[] functionInput)
        {
            return await CreateTransaction(web, address, BigInteger.Zero, function, functionInput);
        }


    }
}
