﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Hoard.Utils.QR
{
    /// <summary>
    /// Bitmap generator from QR code
    /// </summary>
    public class BitmapByteQRCode : AbstractQRCode, IDisposable
    {
        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public BitmapByteQRCode() { }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="data">data to be converted to graphics</param>
        public BitmapByteQRCode(QRCodeData data) : base(data) { }

        /// <summary>
        /// Returns B/W bitmap data with desired resolution
        /// </summary>
        /// <param name="pixelsPerModule">number of pixels per module</param>
        /// <returns></returns>
        public byte[] GetGraphic(int pixelsPerModule)
        {
            return GetGraphic(pixelsPerModule, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0xFF, 0xFF, 0xFF });
        }

        /// <summary>
        /// Returns bitmap data with desired resolution and coloring
        /// </summary>
        /// <param name="pixelsPerModule">numere of pixels per module</param>
        /// <param name="darkColorHtmlHex">bg color</param>
        /// <param name="lightColorHtmlHex">fore color</param>
        /// <returns></returns>
        public byte[] GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex)
        {
            return GetGraphic(pixelsPerModule, HexColorToByteArray(darkColorHtmlHex), HexColorToByteArray(lightColorHtmlHex));
        }

        /// <summary>
        /// Returns bitmap data with desired resolution and coloring
        /// </summary>
        /// <param name="pixelsPerModule">numere of pixels per module</param>
        /// <param name="darkColorRgb">bg color</param>
        /// <param name="lightColorRgb">fore color</param>
        /// <returns></returns>
        public byte[] GetGraphic(int pixelsPerModule, byte[] darkColorRgb, byte[] lightColorRgb)
        {
            var sideLength = this.QrCodeData.ModuleMatrix.Count * pixelsPerModule;

            List<byte> bmp = new List<byte>();

            //header
            bmp.AddRange(new byte[] { 0x42, 0x4D, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1A, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00 });

            //width
            bmp.AddRange(IntTo4Byte(sideLength));
            //height
            bmp.AddRange(IntTo4Byte(sideLength));

            //header end
            bmp.AddRange(new byte[] { 0x01, 0x00, 0x18, 0x00 });

            //draw qr code
            bmp.AddRange(GetRawRGBGraphic(pixelsPerModule, darkColorRgb, lightColorRgb, (sideLength+3)&(~3)));

            //finalize with terminator
            bmp.AddRange(new byte[] { 0x00, 0x00 });

            return bmp.ToArray();
        }

        /// <summary>
        /// Returns raw rgb data with desired resolution and coloring
        /// </summary>
        /// <param name="pixelsPerModule">numere of pixels per module</param>
        /// <param name="darkColorRgb">bg color</param>
        /// <param name="lightColorRgb">fore color</param>
        /// <param name="pitch">line pitch</param>
        /// <returns></returns>
        public byte[] GetRawRGBGraphic(int pixelsPerModule, byte[] darkColorRgb, byte[] lightColorRgb, int pitch)
        {
            var sideLength = this.QrCodeData.ModuleMatrix.Count * pixelsPerModule;

            var moduleDark = darkColorRgb.Reverse();
            var moduleLight = lightColorRgb.Reverse();

            List<byte> bmp = new List<byte>();

            //draw qr code
            for (var x = sideLength - 1; x >= 0; x = x - pixelsPerModule)
            {
                for (int pm = 0; pm < pixelsPerModule; pm++)
                {
                    for (var y = 0; y < sideLength; y = y + pixelsPerModule)
                    {
                        var module =
                            this.QrCodeData.ModuleMatrix[(x + pixelsPerModule) / pixelsPerModule - 1][(y + pixelsPerModule) / pixelsPerModule - 1];
                        for (int i = 0; i < pixelsPerModule; i++)
                        {
                            bmp.AddRange(module ? moduleDark : moduleLight);
                        }
                    }
                    for (int i = sideLength; i < pitch; i++)
                    {
                        bmp.Add(0x00);
                    }
                }
            }

            return bmp.ToArray();
        }

        private byte[] HexColorToByteArray(string colorString)
        {
            if (colorString.StartsWith("#"))
                colorString = colorString.Substring(1);
            byte[] byteColor = new byte[colorString.Length / 2];
            for (int i = 0; i < byteColor.Length; i++)
                byteColor[i] = byte.Parse(colorString.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);            
            return byteColor;
        }

        private byte[] IntTo4Byte(int inp)
        {
            byte[] bytes = new byte[2];
            unchecked
            {
                bytes[1] = (byte)(inp >> 8);
                bytes[0] = (byte)(inp);
            }
            return bytes;
        }
    }
}
