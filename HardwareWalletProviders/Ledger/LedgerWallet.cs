using Device.Net;
using Hid.Net;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static Hoard.HW.Ledger.APDU;

namespace Hoard.HW.Ledger
{
    /// <summary>
    /// Base class for LedgerWallet access. Implements IProfileService
    /// </summary>
    public abstract class LedgerWallet : IProfileService
    {
        /// <summary>
        /// Name of this Wallet type
        /// </summary>
        public const string AccountInfoName = "LedgerWallet";

        /// <summary>
        /// HID device accessor
        /// </summary>
        public IDevice HIDDevice { get; }
        /// <summary>
        /// Path name for this wallet
        /// </summary>
        public string DerivationPath { get; }

        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private const int DEFAULT_CHANNEL = 0x0101;
        private const int LEDGER_HID_PACKET_SIZE = 64;
        private const int TAG_APDU = 0x05;

        /// <summary>
        /// Creates new instance of LedgerWallet base class
        /// </summary>
        /// <param name="hidDevice">HID device accessor</param>
        /// <param name="derivationPath">path name for specific wallet</param>
        protected LedgerWallet(IDevice hidDevice, string derivationPath)
        {
            HIDDevice = hidDevice;
            DerivationPath = derivationPath;
        }

        /// <ineritdoc/>
        public async Task<Profile> CreateProfile(string name) { return null; }

        /// <ineritdoc/>
        abstract public Task<Profile> RequestProfile(string name);

        /// <ineritdoc/>
        abstract public Task<string> SignTransaction(byte[] rlpEncodedTransaction, Profile profile);

        /// <ineritdoc/>
        abstract public Task<string> SignMessage(byte[] message, Profile profile);

        //-------------------------
        private static byte[] GetRequestDataPacket(Stream stream, int packetIndex)
        {
            using (var memory = new MemoryStream())
            {
                var position = (int)memory.Position;
                memory.WriteByte((DEFAULT_CHANNEL >> 8) & 0xff);
                memory.WriteByte(DEFAULT_CHANNEL & 0xff);
                memory.WriteByte(TAG_APDU);
                memory.WriteByte((byte)((packetIndex >> 8) & 0xff));
                memory.WriteByte((byte)(packetIndex & 0xff));

                if (packetIndex == 0)
                {
                    memory.WriteByte((byte)((stream.Length >> 8) & 0xff));
                    memory.WriteByte((byte)(stream.Length & 0xff));
                }

                var headerLength = (int)(memory.Position - position);
                var blockLength = Math.Min(LEDGER_HID_PACKET_SIZE - headerLength, (int)stream.Length - (int)stream.Position);

                byte[] packetBytes = stream.ReadBytes(blockLength);

                memory.Write(packetBytes, 0, packetBytes.Length);

                while ((memory.Length % LEDGER_HID_PACKET_SIZE) != 0)
                {
                    memory.WriteByte(0);
                }

                return memory.ToArray();
            }
        }

        private static byte[] GetResponseDataPacket(byte[] data, int packetIndex, ref int remaining)
        {
            using (var memory = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                {
                    var position = (int)input.Position;

                    //skip channel
                    input.ReadByte();
                    input.ReadByte();

                    var thirdByte = input.ReadByte();
                    if (thirdByte != TAG_APDU)
                    {
                        throw new Exception("Reading from the Ledger failed.");
                    }

                    var fourthByte = input.ReadByte();
                    var expectedResult = (packetIndex >> 8) & 0xff;
                    if (fourthByte != expectedResult)
                    {
                        throw new Exception("Reading from the Ledger failed.");
                    }

                    var fifthByte = input.ReadByte();
                    expectedResult = packetIndex & 0xff;
                    if (fifthByte != expectedResult)
                    {
                        throw new Exception("Reading from the Ledger failed.");
                    }

                    if (packetIndex == 0)
                    {
                        remaining = ((input.ReadByte()) << 8);
                        remaining |= input.ReadByte();
                    }

                    var headerSize = input.Position - position;
                    var blockSize = (int)Math.Min(remaining, LEDGER_HID_PACKET_SIZE - headerSize);

                    var commandPart = new byte[blockSize];
                    if (input.Read(commandPart, 0, commandPart.Length) != commandPart.Length)
                    {
                        throw new Exception("Reading from the Ledger failed. The data read was not of the correct size. It is possible that the incorrect Hid device has been used. Please check that the Hid device with the correct UsagePage was selected");
                    }

                    memory.Write(commandPart, 0, commandPart.Length);

                    remaining -= blockSize;

                    return memory.ToArray();
                }
            }
        }

        private async Task WriteRequestAsync(byte[] message)
        {
            var packetIndex = 0;
            byte[] data = null;
            using (var memory = new MemoryStream(message))
            {
                do
                {
                    data = GetRequestDataPacket(memory, packetIndex);
                    packetIndex++;
                    await HIDDevice.WriteAsync(data);
                } while (memory.Position != memory.Length);
            }
        }

        private async Task<byte[]> ReadResponseAsync()
        {
            var remaining = 0;
            var packetIndex = 0;

            using (var response = new MemoryStream())
            {
                do
                {
                    var packetData = await HIDDevice.ReadAsync();
                    var responseDataPacket = GetResponseDataPacket(packetData, packetIndex, ref remaining);
                    packetIndex++;

                    if (responseDataPacket == null)
                    {
                        return null;
                    }

                    response.Write(responseDataPacket, 0, responseDataPacket.Length);

                } while (remaining != 0);

                return response.ToArray();
            }
        }

        internal async Task<OutputData> SendRequestAsync(byte[] apduRequest)
        {
            await _lock.WaitAsync();

            try
            {
                await WriteRequestAsync(apduRequest);
                var response = await ReadResponseAsync();
                var statusCode = ((response[response.Length - 2] & 0xff) << 8) | response[response.Length - 1] & 0xff;
                return new OutputData { Data = response, StatusCode = statusCode };
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Checks if given code is a success
        /// </summary>
        /// <param name="code">code to check</param>
        /// <returns>true if code is a successful operation, false otherwise</returns>
        protected static bool IsSuccess(int code)
        {
            return (code == 0x9000);
        }

        /// <summary>
        /// Returns a descriptive message of given result code
        /// </summary>
        /// <param name="code">code to get description for</param>
        /// <returns></returns>
        protected static string GetStatusMessage(int code)
        {
            switch (code)
            {
                case 0x9000:
                    return "Success";
                case 0x6D00:
                    return "Instruction not supported in current app or there is no app running";
                case 0x6B00:
                    return "Invalid parameter";
                case 0x6A80:
                    return "The data is invalid";
                case 0x6804:
                    return "Unknown error. Possibly from Firmware?";
                case 0x6E00:
                    return "CLA not supported in current app";
                case 0x6700:
                    return "Data length is incorrect";
                case 0x6982:
                    return "The security is not valid for this command";
                case 0x6985:
                    return "Conditions have not been satisfied for this command";
                case 0x6482:
                    return "File not found";
                default:
                    return "Internal error";
            }
        }
    }
}
