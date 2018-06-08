
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

    // TODO: move CryptoKittyProvider to separate plugin/library!
    public class CryptoKittyProvider : Provider
    {
        private HoardService hoard;

        private string[] properties = new string[1] { "genotype" };

        public CryptoKittyProvider(HoardService _hoard)
        {
            hoard = _hoard;
        }

        override public string[] getPropertyNames()
        {
            return properties;
        }

        override public Result getItems(out GameAsset[] items)
        {
            // TODO: get crypto kitties owned by address from BC

            var gameAsset = new GameAsset(
                "CK", //symbol
                "CryptoKitties", //name
                null, //TODO: contract
                1, //totalSupply
                4823947, //assetId
                "cryptokitty"); //assetType

            items = new GameAsset[1];
            items[0] = gameAsset;

            return new Result();
        }

        override public Result getProperties(GameAsset item, out Property[] props)
        {
            if (item.AssetType == "cryptokitty")
            {
                // TODO: get cryptokitty genotype by cryptokitty id from BC

                props = new Property[1];
                props[0] = new Property { name = properties[0], data = new byte[32] };

                for (int i = 0; i < props[0].data.Length; ++i)
                    props[0].data[i] = 0x0;

                return new Result();
            }

            props = null;
            return new Result("no props");
        }

        override public Result getProperties(GameAsset item, string name, out Property[] props)
        {
            if (name == properties[0])
                return getProperties(item, out props);

            props = null;
            return new Result("DataProvider doesn't support '" + name + "'");
        }
    }
}
