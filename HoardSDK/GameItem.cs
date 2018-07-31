using Hoard.Utils;
using System;
using System.Collections.Generic;

namespace Hoard
{
    public class ItemProperty
    {
        public object Value;
        public string Type;

        public ItemProperty(object value, string type)
        {
            Value = value;
            Type = type;
        }

        public Type GetType()
        {
            if(Type.Equals("int16"))
                return System.Type.GetType("System.Int16");
            else if(Type.Equals("int32"))
                return System.Type.GetType("System.Int32");
            else if (Type.Equals("int64"))
                return System.Type.GetType("System.Int64");
            else if (Type.Equals("uint16"))
                return System.Type.GetType("System.UInt16");
            else if (Type.Equals("uint32"))
                return System.Type.GetType("System.UInt32");
            else if (Type.Equals("uint64"))
                return System.Type.GetType("System.UInt64");
            else if (Type.Equals("string"))
                return System.Type.GetType("System.String");
            else if (Type.Equals("bool"))
                return System.Type.GetType("System.Boolean");
            else if (Type.Equals("float"))
                return System.Type.GetType("System.Single");
            else if (Type.Equals("double"))
                return System.Type.GetType("System.Double");
            return System.Type.GetType("System.String");
        }
    }

    public class ItemProperties : Dictionary<string, ItemProperty>
    {

        public ItemProperty GetItemProperty(string name)
        {
            if(ContainsKey(name))
            {
                return this[name];
            }
            return null;
        }

        public void Add(string name, object value, string type)
        {
            this[name] = new ItemProperty(value, type);
        }

       
    }

    public interface IGameItemMetadata
    {
        TResult Get<TResult>(string name);

        void Set<TResult>(string name, TResult value);
    }

    public abstract class BaseGameItemMetadata : IGameItemMetadata
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

    public class GameItem
    {
        public string Symbol { get; private set; }
        public string Checksum { get; set; } = "0x0";
        public IGameItemMetadata Metadata { get; set; } = null;
        public ItemProperties Properties { get; set; } = new ItemProperties();

        public GameItem(string symbol, IGameItemMetadata metadata)
        {
            Symbol = symbol;
            Metadata = metadata;
        }
    }
}
