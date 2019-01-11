using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// User input type enumeration for UserInputProvider implementations
    /// </summary>
    public enum eUserInputType
    {
        /// <summary>
        /// Login field (user name, etc.)
        /// </summary>
        kLogin = 0,
        /// <summary>
        /// Password field
        /// </summary>
        kPassword,
        /// <summary>
        /// PIN (useful in wallet implementations)
        /// </summary>
        kPIN,
        /// <summary>
        /// Email field
        /// </summary>
        kEmail,
        /// <summary>
        /// Any other field. functionlaity or typed should be deduced from description
        /// </summary>
        kCustom
    }

    /// <summary>
    /// User Input interface for communicationg with GUI applications and/or passing
    /// information from User
    /// </summary>
    public interface IUserInputProvider
    {
        /// <summary>
        /// Requests input from User.
        /// </summary>
        /// <param name="user">User from wchich we want some input</param>
        /// <param name="type">What kind of input</param>
        /// <param name="description">Human readable description (comment)</param>
        /// <returns>Input from User</returns>
        Task<string> RequestInput(User user, eUserInputType type, string description);
    }
}
