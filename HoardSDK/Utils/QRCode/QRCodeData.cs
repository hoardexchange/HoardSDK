using System.Collections;
using System.Collections.Generic;

namespace Hoard.Utils.QR
{
    using System;

    /// <summary>
    /// QR Code data representation
    /// </summary>
    public class QRCodeData : IDisposable
    {
        /// <summary>
        /// Bit array matrix with modules
        /// </summary>
        public List<BitArray> ModuleMatrix { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="version">version determines number of modules</param>
        public QRCodeData(int version)
        {
            this.Version = version;
            var size = ModulesPerSideFromVersion(version);
            this.ModuleMatrix = new List<BitArray>();
            for (var i = 0; i < size; i++)
                this.ModuleMatrix.Add(new BitArray(size));
        }

        /// <summary>
        /// Constructs QR code from raw data
        /// </summary>
        /// <param name="rawData">data to be encoded in QRR format</param>
        public QRCodeData(byte[] rawData)
        {
            var bytes = new List<byte>(rawData);
            
            if (bytes[0] != 0x51 || bytes[1] != 0x52 || bytes[2] != 0x52)
                throw new Exception("Invalid raw data file. Filetype doesn't match \"QRR\".");

            //Set QR code version
            var sideLen = (int)bytes[4];
            bytes.RemoveRange(0, 5);
            this.Version = (sideLen - 21 - 8) / 4 + 1;

            //Unpack
            var modules = new Queue<bool>();
            foreach (var b in bytes)
            {
                var bArr = new BitArray(new byte[] { b });
                for (int i = 7; i >= 0; i--)
                {
                    modules.Enqueue((b & (1 << i)) != 0);
                }
            }

            //Build module matrix
            this.ModuleMatrix = new List<BitArray>();
            for (int y = 0; y < sideLen; y++)
            {
                this.ModuleMatrix.Add(new BitArray(sideLen));
                for (int x = 0; x < sideLen; x++)
                {
                    this.ModuleMatrix[y][x] = modules.Dequeue();
                }
            }
        }

        /// <summary>
        /// Returns data in QRR format
        /// </summary>
        /// <returns>uncompressed QRR encoded data</returns>
        public byte[] GetRawData()
        {
            var bytes = new List<byte>();

            //Add header - signature ("QRR")
            bytes.AddRange(new byte[]{ 0x51, 0x52, 0x52, 0x00 });

            //Add header - rowsize
            bytes.Add((byte)ModuleMatrix.Count);

            //Build data queue
            var dataQueue = new Queue<int>();
            foreach (var row in ModuleMatrix)
            {
                foreach (var module in row)
                {
                    dataQueue.Enqueue((bool)module ? 1 : 0);
                }
            }
            for (int i = 0; i < 8 - (ModuleMatrix.Count * ModuleMatrix.Count) % 8; i++)
            {
                dataQueue.Enqueue(0);
            }

            //Process queue
            while (dataQueue.Count > 0)
            {
                byte b = 0;
                for (int i = 7; i >= 0; i--)
                {
                    b += (byte)(dataQueue.Dequeue() << i);
                }
                bytes.Add(b);
            }
            var rawData = bytes.ToArray();

            return rawData;
        }

        /// <summary>
        /// Version of QR code
        /// </summary>
        public int Version { get; private set; }

        private static int ModulesPerSideFromVersion(int version)
        {
            return 21 + (version - 1) * 4;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.ModuleMatrix = null;
            this.Version = 0;

        }
    }
}
