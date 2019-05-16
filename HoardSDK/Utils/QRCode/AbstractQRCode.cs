﻿namespace Hoard.Utils.QR
{
    /// <summary>
    /// Base class for QR code generators
    /// </summary>
    public abstract class AbstractQRCode
    {
        /// <summary>
        /// QRCodeData object
        /// </summary>
        protected QRCodeData QrCodeData { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        protected AbstractQRCode() {
        }

        /// <summary>
        /// Constructor with initializer data
        /// </summary>
        /// <param name="data">qr code data</param>
        protected AbstractQRCode(QRCodeData data) {
            this.QrCodeData = data;
        }

        /// <summary>
        /// Set a QRCodeData object that will be used to generate QR code. Used in COM Objects connections
        /// </summary>
        /// <param name="data">Need a QRCodeData object generated by QRCodeGenerator.CreateQrCode()</param>
        virtual public void SetQRCodeData(QRCodeData data) {
            this.QrCodeData = data;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.QrCodeData = null;
        }
    }
}