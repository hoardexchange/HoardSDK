using System;

namespace Hoard.Exceptions
{
    /// <summary>
    /// Hoard sdk exception
    /// </summary>
    public class HoardException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public HoardException()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public HoardException(string message)
            : base(message)
        {
            ErrorCallbackProvider.ReportError(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public HoardException(string message, Exception inner)
            : base(message, inner)
        {
            ErrorCallbackProvider.ReportError(message);
        }
    }
}
