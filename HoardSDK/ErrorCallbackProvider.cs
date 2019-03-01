using System;
using System.Collections.Generic;
using System.Text;

namespace Hoard
{
    /// <summary>
    /// Error callback provider (not thread safe)
    /// </summary>
    public static class ErrorCallbackProvider
    {
        /// <summary>
        /// Message type
        /// </summary>
        public enum MessagType
        {
            /// <summary>
            /// Info
            /// </summary>
            Info = 0,

            /// <summary>
            /// Warning
            /// </summary>
            Warning,
            /// <summary>
            /// Error
            /// </summary>
            Error
        }

        /// <summary>
        /// Error callback
        /// </summary>
        public delegate void ErrorCallback(string message, MessagType type);

        /// <summary>
        /// Event raised when error is reported
        /// </summary>
        public static event ErrorCallback OnReport;

        /// <summary>
        /// Report error
        /// </summary>
        /// <param name="message"></param>
        public static void ReportError(string message)
        {
            if (OnReport != null)
            {
                OnReport(message, MessagType.Error);
            }
        }

        /// <summary>
        /// Report warning
        /// </summary>
        /// <param name="message"></param>
        public static void ReportWarning(string message)
        {
            if (OnReport != null)
            {
                OnReport(message, MessagType.Warning);
            }
        }

        /// <summary>
        /// Report info
        /// </summary>
        /// <param name="message"></param>
        public static void ReportInfo(string message)
        {
            if (OnReport != null)
            {
                OnReport(message, MessagType.Info);
            }
        }
    }
}
