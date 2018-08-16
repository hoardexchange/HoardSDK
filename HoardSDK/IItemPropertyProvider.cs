using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Provider for Item Properties interface.
    /// </summary>
    public interface IItemPropertyProvider
    {
        /// <summary>
        /// Checks if this provider supports decoding this particular state type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool Supports(string type);

        /// <summary>
        /// Updates game item properties and updates game item. Synchronous function.
        /// Warning: might take long time to execute.
        /// </summary>
        bool UpdateGameItemProperties(GameItem item);
    }
}
