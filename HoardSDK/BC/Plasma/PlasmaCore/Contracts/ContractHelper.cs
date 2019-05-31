using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.NonceServices;
using Nethereum.Web3;
using System.Numerics;
using System.Threading.Tasks;

namespace Plasma.RootChain.Contracts
{
    /// <summary>
    /// Contract creation helper
    /// </summary>
    public static class ContractHelper
    {
        public static async Task<Nethereum.Signer.Transaction> CreateTransaction(Web3 web, string addressFrom, BigInteger amount, Function function, params object[] functionInput)
        {
            var gasPrice = await web.Eth.GasPrice.SendRequestAsync();
            HexBigInteger gas = await function.EstimateGasAsync(addressFrom.EnsureHexPrefix(), gasPrice, new HexBigInteger(amount), functionInput);

            var nonceService = new InMemoryNonceService(addressFrom.EnsureHexPrefix(), web.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();

            string data = function.GetData(functionInput);
            var transaction = new Nethereum.Signer.Transaction(function.ContractAddress, amount, nonce, gasPrice, gas.Value, data);

            return transaction;
        }

        public static async Task<Nethereum.Signer.Transaction> CreateTransaction<TFunctionInput>(Web3 web, string addressFrom, BigInteger amount, Function<TFunctionInput> function, TFunctionInput functionInput)
        {
            var gasPrice = await web.Eth.GasPrice.SendRequestAsync();
            HexBigInteger gas = await function.EstimateGasAsync(functionInput, addressFrom.EnsureHexPrefix(), gasPrice, new HexBigInteger(amount));

            var nonceService = new InMemoryNonceService(addressFrom.EnsureHexPrefix(), web.Client);
            BigInteger nonce = await nonceService.GetNextNonceAsync();

            string data = function.GetData(functionInput);
            var transaction = new Nethereum.Signer.Transaction(function.ContractAddress, amount, nonce, gasPrice, gas.Value, data);

            return transaction;
        }

        public static async Task<Nethereum.Signer.Transaction> CreateTransaction(Web3 web, string address, Function function, params object[] functionInput)
        {
            return await CreateTransaction(web, address, BigInteger.Zero, function, functionInput);
        }
    }
}
