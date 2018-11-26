using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Authentication service access interface.
    /// Used to authenticate user
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Descriptive name of this service.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Logins a user.
        /// </summary>
        /// <returns></returns>
        Task<User> LoginUser();
    }
}
