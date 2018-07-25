using Hoard.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Hoard
{
    public enum PropertyType
    {
        Unknown = 0,
        String,
        Address,
        Bool,
        Int16,
        Int32,
        Int64,
        Uint16,
        Uint32,
        Uint64,
        BigInt,
    }

    public struct ItemCRC
    {
        public ulong crc;
    }

    public class ItemData
    {
        public byte[] BinaryData;
        public ItemCRC DataCRC;
    }

    public class Prop
    {
        public PropertyType type = PropertyType.Unknown;
        public object value;
    }

    // Props holds set of values identified individually by string, it can by anything like single values, custom objects or binary data
    public class Props
    {
        public Dictionary<string, Prop> Properties { get; set; } = new Dictionary<string, Prop>();

        public object Get(string propertyName)
        {
            Prop prop;
            Properties.TryGetValue(propertyName, out prop);
            return prop.value;
        }

        public void Set(string propertyName, object propertyValue, PropertyType type = PropertyType.Unknown)
        {
            Prop prop;
            if (Properties.TryGetValue(propertyName, out prop) == false)
            {
                Register(propertyName, propertyValue, type);
            }
            else
            {
                prop.value = propertyValue;
            }
        }

        protected void Register(string propertyName, object propertyValue, PropertyType type)
        {
            Properties[propertyName] = new Prop();
            Properties[propertyName].value = propertyValue;
            Properties[propertyName].type = type;
        }
    }

    public class PropsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public interface IGameAssetMetadata
    {
        TResult Get<TResult>(string name);
        void Set<TResult>(string name, TResult value);
    }

    public abstract class BaseGameAssetMetadata : IGameAssetMetadata
    {
        public TResult Get<TResult>(string name)
        {
            return this.GetPropertyValue<TResult>(name);
        }

        public void Set<TResult>(string name, TResult value)
        {
            this.SetPropertyValue<TResult>(name, value);
        }
    }

    public class GameAsset
    {
        public string AssetSymbol { get; private set; }
        public Props Properties { get; set; } = new Props();
        public IGameAssetMetadata Metadata { get; set; } = null;

        public GameAsset(string assetSymbol, Props properties, IGameAssetMetadata metadata = null)
        {
            AssetSymbol = assetSymbol;
            Properties = properties;
            Metadata = metadata;
        }
    }
}
