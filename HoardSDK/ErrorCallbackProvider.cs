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
        public static event ErrorCallback OnReportError;

        /// <summary>
        /// Event raised when warning is reported
        /// </summary>
        public static event ErrorCallback OnReportWarning;

        /// <summary>
        /// Event raised when info is reported
        /// </summary>
        public static event ErrorCallback OnReportInfo;

        /// <summary>
        /// Report error
        /// </summary>
        /// <param name="message"></param>
        public static void ReportError(string message)
        {
            if (OnReportError != null)
            {
                OnReportError(message);
            }
        }

        /// <summary>
        /// Report warning
        /// </summary>
        /// <param name="message"></param>
        public static void ReportWarning(string message)
        {
            if (OnReportWarning != null)
            {
                OnReportWarning(message);
            }
        }

        /// <summary>
        /// Report info
        /// </summary>
        /// <param name="message"></param>
        public static void ReportInfo(string message)
        {
            if (OnReportInfo != null)
            {
                OnReportInfo(message);
            }
        }
    }
}
