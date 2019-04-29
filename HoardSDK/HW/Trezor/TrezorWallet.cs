using Hid.Net;
using Hoard.HW.Trezor.Ethereum;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard.HW.Trezor
{
    /// <summary>
    /// Base class for TrezorWallet access. Implements IAccountService
    /// </summary>
    public abstract class TrezorWallet : IProfileService
    {
        /// <summary>
        /// Name of this Wallet type
        /// </summary>
        public const string AccountInfoName = "TrezorWallet";

        private const int FirstChunkResponseIdx = 9;

        /// <summary>
        /// HID device accessor
        /// </summary>
        public IHidDevice HIDDevice { get; }
        /// <summary>
        /// Path name for this wallet
        /// </summary>
        public string DerivationPath { get; }
        internal Features Features { get; private set; }

        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private object lastRequest;

        private IUserInputProvider pinInputProvider;

        /// <summary>
        /// Creates new instance of LedgerWallet base class
        /// </summary>
        /// <param name="hidDevice">HID device accessor</param>
        /// <param name="derivationPath">path name for specific wallet</param>
        /// <param name="_pinInputProvider">provider for PIN</param>
        protected TrezorWallet(IHidDevice hidDevice, string derivationPath, IUserInputProvider _pinInputProvider)
        {
            HIDDevice = hidDevice;
            DerivationPath = derivationPath;
            pinInputProvider = _pinInputProvider;
        }

        /// <ineritdoc/>
        public async Task<Profile> CreateProfile(string name) { return null; }

        /// <ineritdoc/>
        abstract public Task<Profile> RequestProfile(string name);

        /// <ineritdoc/>
        abstract public Task<string> SignTransaction(byte[] rlpEncodedTransaction, Profile profile);

        /// <ineritdoc/>
        abstract public Task<string> SignMessage(byte[] message, Profile profile);

        /// <summary>
        /// Initializes the wallet. Call this when first using Trezor device.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            var response = await SendRequestAsync(new InitializeRequest());
            if (response == null)
            {
                throw new Exception("Error initializing Trezor. Features were not retrieved");
            }
        }

        /// <summary>
        /// Sends current request with given PIN.
        /// </summary>
        /// <param name="pin">PIN to send request with</param>
        /// <returns>response of request if PIN correct, throws InvalidPIN exception otherwise</returns>
        protected async Task<object> PinMatrixAckAsync(string pin)
        {
            var response = await SendRequestAsyncNoLock(new PinMatrixAck { Pin = pin });
            if (response is Failure)
            {
                throw new Exception("Invalid PIN.");
            }
            return response;
        }

        /// <summary>
        /// Sends current request with button acknowledgment
        /// </summary>
        /// <returns></returns>
        protected async Task<object> ButtonAckAsync()
        {
            var response = await SendRequestAsyncNoLock(new ButtonAck());
            if (response is Failure)
            {
                throw new Exception("ButtonAck failed.");
            }

            return response;
        }

        /// <summary>
        /// Returns MessageType from string name
        /// </summary>
        /// <param name="messageTypeString">string name of MessageType value</param>
        /// <returns>Parsed MessageType value or throws ArgumentException if argument is not correct</returns>
        protected object GetEnumValue(string messageTypeString)
        {
            var isValid = Enum.TryParse(messageTypeString, out MessageType messageType);
            if (!isValid)
            {
                throw new ArgumentException($"{messageTypeString} is not a valid MessageType");
            }

            return messageType;
        }

        private async Task WriteRequestAsync(object request)
        {
            var byteArray = Helpers.ProtoBufSerialize(request);
            var size = byteArray.Length;

            var id = (int)GetEnumValue("MessageType" + request.GetType().Name);
            var data = new byte[size + 1024]; // 32768);
            data[0] = (byte)'#';
            data[1] = (byte)'#';
            data[2] = (byte)((id >> 8) & 0xFF);
            data[3] = (byte)(id & 0xFF);
            data[4] = (byte)((size >> 24) & 0xFF);
            data[5] = (byte)((size >> 16) & 0xFF);
            data[6] = (byte)((size >> 8) & 0xFF);
            data[7] = (byte)(size & 0xFF);
            byteArray.CopyTo(data, 8);

            var position = size + 8;
            while (position % 63 > 0)
            {
                data[position] = 0;
                position++;
            }

            for (var i = 0; i < (position / 63); i++)
            {
                var chunk = new byte[64];
                chunk[0] = (byte)'?';

                for (var x = 0; x < 63; x++)
                {
                    chunk[x + 1] = data[(i * 63) + x];
                }

                await HIDDevice.WriteAsync(chunk);
            }

            lastRequest = request;
        }

        private async Task<object> ReadResponseAsync()
        {
            var data = await HIDDevice.ReadAsync();

            var firstByteNot63 = data[0] != (byte)'?';
            var secondByteNot35 = data[1] != 35;
            var thirdByteNot35 = data[2] != 35;
            if (firstByteNot63 || secondByteNot35 || thirdByteNot35)
            {
                throw new Exception("An error occurred while attempting to read the message from the device.");
            }

            var typeInt = data[4];
            if (!Enum.IsDefined(typeof(MessageType), (int)typeInt))
            {
                throw new Exception("Not a valid MessageType");
            }

            var messageType = (MessageType)Enum.Parse(typeof(MessageType), Enum.GetName(typeof(MessageType), typeInt));

            var remainingDataLength = ((data[5] & 0xFF) << 24)
                                    + ((data[6] & 0xFF) << 16)
                                    + ((data[7] & 0xFF) << 8)
                                    + (data[8] & 0xFF);

            var length = Math.Min(data.Length - (FirstChunkResponseIdx), remainingDataLength);

            var allData = data.ToList().GetRange(FirstChunkResponseIdx, length);
            remainingDataLength -= allData.Count;

            var invalidChunksCounter = 0;

            while (remainingDataLength > 0)
            {
                data = await HIDDevice.ReadAsync();

                if (data.Length <= 0)
                {
                    continue;
                }

                length = Math.Min(data.Length, remainingDataLength);

                if (data[0] != (byte)'?')
                {
                    if (invalidChunksCounter++ > 5)
                    {
                        throw new Exception("messageRead: too many invalid chunks (2)");
                    }
                }

                allData.InsertRange(allData.Count, data.ToList().GetRange(1, length - 1));
                remainingDataLength -= (length - 1);

                //Super hack! Fix this!
                if (remainingDataLength != 1)
                {
                    continue;
                }

                allData.InsertRange(allData.Count, data.ToList().GetRange(length, 1));
                remainingDataLength = 0;
            }

            return Helpers.ProtoBufDeserialize(GetContractType(messageType), allData.ToArray());
        }

        internal async Task<object> SendRequestAsync(object request)
        {
            await _lock.WaitAsync();

            try
            {
                return await SendRequestAsyncNoLock(request);
            }
            finally
            {
                _lock.Release();
            }
        }

        internal async Task<object> SendRequestAsyncNoLock(object request)
        {
            await WriteRequestAsync(request);
            var response = await ReadResponseAsync();

            while (response is PinMatrixRequest || response is ButtonRequest)
            {
                if (response is PinMatrixRequest)
                {
                    var pin = await pinInputProvider.RequestInput(null, null, eUserInputType.kPIN, "Trezor PIN request");
                    response = await PinMatrixAckAsync(pin);
                }
                else if (response is ButtonRequest)
                {
                    response = await ButtonAckAsync();
                }
            }

            return response;
        }

        private Type GetContractType(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.MessageTypeButtonAck:
                    return typeof(ButtonAck);
                case MessageType.MessageTypeButtonRequest:
                    return typeof(ButtonRequest);
                case MessageType.MessageTypePinMatrixAck:
                    return typeof(PinMatrixAck);
                case MessageType.MessageTypePinMatrixRequest:
                    return typeof(PinMatrixRequest);
                case MessageType.MessageTypeEthSignTransactionRequest:
                    return typeof(EthSignTransactionRequest);
                case MessageType.MessageTypeEthSignTransactionResponse:
                    return typeof(EthSignTransactionResponse);
                case MessageType.MessageTypeEthSignMessageRequest:
                    return typeof(EthSignMessageRequest);
                case MessageType.MessageTypeEthSignMessageResponse:
                    return typeof(EthSignMessageResponse);
                case MessageType.MessageTypeEthAddressRequest:
                    return typeof(EthAddressRequest);
                case MessageType.MessageTypeEthAddressResponse:
                    return typeof(EthAddressResponse);
                case MessageType.MessageTypeInitializeRequest:
                    return typeof(InitializeRequest);
                case MessageType.MessageTypeFeatures:
                    return typeof(Features);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
