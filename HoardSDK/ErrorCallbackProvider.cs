using System;
using System.Collections.Generic;
using System.Text;

namespace Hoard
{
    /// <summary>
    /// Error callback provider
    /// </summary>
    public class ErrorCallbackProvider
    {
        /// <summary>
        /// Error callback
        /// </summary>
        public class IErrorCallback
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public IErrorCallback()
            {
                Instance.RegisterCallback(this);
            }

            /// <summary>
            /// Destructor
            /// </summary>
            ~IErrorCallback()
            {
                Instance.UnregisterCallback();
            }

            /// <summary>
            /// Reports error
            /// </summary>
            /// <param name="code"></param>
            /// <param name="message"></param>
            public virtual void ReportError(string message) {}
        }

        private IErrorCallback ErrorCallback = null;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static readonly ErrorCallbackProvider Instance = new ErrorCallbackProvider();

        /// <summary>
        /// Explicit static constructor to tell C# compiler
        /// </summary>
        static ErrorCallbackProvider()
        {
        }

        private ErrorCallbackProvider()
        {
        }

        private void RegisterCallback(IErrorCallback callback)
        {
            Instance.ErrorCallback = callback;
        }

        private void UnregisterCallback()
        {
            Instance.ErrorCallback = null;
        }

        /// <summary>
        /// Report error
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public void ReportError(string message)
        {
            if (ErrorCallback != null)
            {
                ErrorCallback.ReportError(message);
            }
        }
    }
}
