using Hoard.Utils;
using System;
using System.Collections.Generic;

namespace Hoard
{
    /// <summary>
    /// Genric property of a Game Item.
    /// </summary>
    public class ItemProperty
    {
        public object Value;
        public string Type;

        public ItemProperty(object value, string type)
        {
            Value = value;
            Type = type;
        }

        public new Type GetType()
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

    /// <summary>
    /// Metadata interface
    /// </summary>
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

    /// <summary>
    /// Representation of Game Item description retrieved from Hoard Platform
    /// </summary>
    public class GameItem
    {
        public GameID Game { get; private set; }
        public string Symbol { get; private set; }
        public byte[] State { get; set; }
        public IGameItemMetadata Metadata { get; set; } = null;
        public ItemProperties Properties { get; set; } = null;

        public GameItem(GameID game, string symbol, IGameItemMetadata metadata)
        {
            Game = game;
            Symbol = symbol;
            Metadata = metadata;
        }
    }
}