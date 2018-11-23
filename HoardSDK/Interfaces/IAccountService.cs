
using System.Threading.Tasks;

namespace Hoard
{
    public interface IAccountService
    {
        /// <summary>
        /// Create new account for User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> CreateAccount(string name, User user);

        /// <summary>
        /// Load all accounts registered for User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> RequestAccounts(User user);

        /// <summary>
        /// Sings any message with account signature
        /// </summary>
        /// <param name="input"></param>
        /// <param name="signature"></param>
        /// <returns>Signed message</returns>
        Task<string> Sign(byte[] input, AccountInfo signature);
    }
}
