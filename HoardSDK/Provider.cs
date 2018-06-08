
namespace Hoard
{
    public class Property
    {
        public string name { get; set; }
        public byte[] data { get; set; }
    }

    abstract public class Provider
    {
        // get property names this provider can provide
        abstract public string[] getPropertyNames();

        // get items
        abstract public Result getItems(out GameAsset[] items);

        // get all properties for given item
        abstract public Result getProperties(GameAsset item, out Property[] props);

        // get properties for given item by property name
        abstract public Result getProperties(GameAsset item, string name, out Property[] props);
    }
}
