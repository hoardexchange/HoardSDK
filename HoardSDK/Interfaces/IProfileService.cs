
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
    }
}
