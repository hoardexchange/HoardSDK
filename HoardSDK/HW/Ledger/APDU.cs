using System;

namespace Hoard.HW.Ledger
{
    internal class APDU
    {
        public struct OutputData
        {
            public byte[] Data { get; set; }
            public int StatusCode { get; set; }
        }

        public static byte[] InputData(byte cla, byte ins, byte p1, byte p2, byte[] input)
        {
            var apdu = new byte[input.Length + 5];
            apdu[0] = cla;
            apdu[1] = ins;
            apdu[2] = p1;
            apdu[3] = p2;
            apdu[4] = (byte)(input.Length);
            Array.Copy(input, 0, apdu, 5, input.Length);
            return apdu;
        }
    }
}
