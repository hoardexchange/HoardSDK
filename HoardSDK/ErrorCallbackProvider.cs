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
        /// Error callback
        /// </summary>
        public delegate void ErrorCallback(string message);

        /// <summary>
        /// Event raised when error is reported
        /// </summary>
        public static event ErrorCallback OnError;

        /// <summary>
        /// Report error
        /// </summary>
        /// <param name="message"></param>
        public static void ReportError(string message)
        {
            if (OnError != null)
            {
                OnError(message);
            }
        }
    }
}
