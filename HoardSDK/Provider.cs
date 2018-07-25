using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hoard
{
    abstract public class Provider
    {
        // FIXME: not needed?
        // get property names this provider can provide
        //abstract public string[] GetPropertyNames();

        // get items
        abstract public Task<List<GameAsset>> GetItems(string ownerAddress, uint page, uint pageSize);

        // FIXME: not needed cos we store properties in GameAsset?
        //// updates all properties for given item
        //abstract public Result GetProperties(GameAsset item);
    }
}
