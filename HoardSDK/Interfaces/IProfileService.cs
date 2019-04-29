
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Service for managing user accounts. 
    /// </summary>
    public interface IProfileService
    {
        /// <summary>
        /// Create new account for User
        /// </summary>
        /// <param name="name">name of new profile</param>
        /// <returns>new profile</returns>
        Task<Profile> CreateProfile(string name);

        /// <summary>
        /// Load all accounts registered for User
        /// </summary>
        /// <param name="name">address or name of profile</param>
        /// <returns></returns>
        Task<Profile> RequestProfile(string name);

        /// <summary>
        /// Sings transaction with account signature
        /// </summary>
        /// <param name="rlpEncodedTransaction"></param>
        /// <param name="profile"></param>
        /// <returns>Signed transaction</returns>
        Task<string> SignTransaction(byte[] rlpEncodedTransaction, Profile profile);

        /// <summary>
        /// Sings any message with account signature
        /// </summary>
        /// <param name="message"></param>
        /// <param name="profile"></param>
        /// <returns>Signed message</returns>
        Task<string> SignMessage(byte[] message, Profile profile);
    }
}
