
using System.Collections.Generic;

namespace Hoard
{
    abstract public class Provider
    {
        // get property names this provider can provide
        abstract public string[] getPropertyNames();

        // get items
        abstract public Result getItems(out List<GameAsset> items);

        // updates all properties for given item
        abstract public Result getProperties(GameAsset item);
    }
}
