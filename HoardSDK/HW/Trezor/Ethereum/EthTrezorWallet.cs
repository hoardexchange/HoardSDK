using Hid.Net;
using System;
using System.Threading.Tasks;

namespace Hoard.HW.Trezor.Ethereum
{
    public class EthTrezorWallet : TrezorWallet
    {
        private KeyPath keyPath;
        private byte[] derivation;
        private uint[] indices;

        public EthTrezorWallet(IHidDevice hidDevice, string derivationPath, IUserInputProvider pinInputProvider, uint index = 0) 
            : base(hidDevice, derivationPath, pinInputProvider)
        {
            keyPath = new KeyPath(derivationPath).Derive(index);
            indices = keyPath.Indices;
            derivation = keyPath.ToBytes();
        }

        public override async Task<bool> RequestAccounts(User user)
        {
            var output = await SendRequestAsync(EthGetAddress.Request(indices));
            var address = EthGetAddress.GetAddress(output);
            user.Accounts.Add(new AccountInfo(AccountInfoName, address, this));
            return true;
        }

        public override async Task<string> SignTransaction(byte[] rlpEncodedTransaction, AccountInfo accountInfo)
        {
            var output = await SendRequestAsync(EthSignTransaction.Request(indices, rlpEncodedTransaction));
            return EthSignTransaction.GetRLPEncoded(output, rlpEncodedTransaction);
        }

        public override async Task<string> SignMessage(byte[] message, AccountInfo accountInfo)
        {
            throw new NotImplementedException();
        }
    }
}
