using Hoard.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Hoard
{
    /// <summary>
    /// Genric property of a Game Item.
    /// </summary>
    public class ItemProperty
    {
        /// <summary>
        /// Type of this property
        /// </summary>
        [JsonProperty(propertyName: "type")]
        public string Type;

        /// <summary>
        /// Value of this property
        /// </summary>
        [JsonProperty(propertyName: "value")]
        public object Value;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="value">value of property</param>
        /// <param name="type">type of property</param>
        public ItemProperty(object value, string type)
        {
            Type = type;
            if (Type.Equals("dict") && value is JObject)
                Value = ((JObject)value).ToObject<ItemProperties>();
            else
                Value = value;
        }

        /// <summary>
        /// Returns type of stored Value object
        /// </summary>
        /// <returns></returns>
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
            else if (Type.Equals("byte[]"))
                return System.Type.GetType("Byte[]");
            else if (Type.Equals("dict"))
                return System.Type.GetType("ItemProperties");
            return System.Type.GetType("System.String");
        }
    }

    /// <summary>
    /// Defines properties of a Game Item
    /// </summary>
    public class ItemProperties : Dictionary<string, ItemProperty>
    {
        /// <summary>
        /// Returns ItemProperty by its key
        /// </summary>
        /// <param name="name">dictionary key</param>
        /// <returns></returns>
        public ItemProperty GetItemProperty(string name)
        {
            if(ContainsKey(name))
            {
                return this[name];
            }
            return null;
        }

        /// <summary>
        /// Adds new Item property
        /// </summary>
        /// <param name="name">dictionary key</param>
        /// <param name="value">value</param>
        /// <param name="type">type of stored object</param>
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
        /// <summary>
        /// Generic Getter for property by name
        /// </summary>
        /// <typeparam name="TResult">Type of this property</typeparam>
        /// <param name="name">name of the property</param>
        /// <returns></returns>
        TResult Get<TResult>(string name);

        /// <summary>
        /// Generic setter for a property
        /// </summary>
        /// <typeparam name="TResult">Type of the property</typeparam>
        /// <param name="name">name of the property</param>
        /// <param name="value">Value of the property to set</param>
        void Set<TResult>(string name, TResult value);
    }

    /// <summary>
    /// Basic implementation of IGameItemMetadata returning ItemProperties
    /// </summary>
    public abstract class BaseGameItemMetadata : IGameItemMetadata
    {
        /// <inheritdoc/>
        public TResult Get<TResult>(string name)
        {
            return this.GetPropertyValue<TResult>(name);
        }

        /// <inheritdoc/>
        public void Set<TResult>(string name, TResult value)
        {
            this.SetPropertyValue<TResult>(name, value);
        }
    }

    /// <summary>
    /// Descriptor of Game Item retrieved from Hoard Platform
    /// </summary>
    public class GameItem
    {
        /// <summary>
        /// Game this item belongs to
        /// </summary>
        public GameID Game { get; private set; }
        /// <summary>
        /// Symbol(type) of this game item
        /// </summary>
        public string Symbol { get; private set; }
        /// <summary>
        /// State of item (arbitrary value)
        /// </summary>
        public byte[] State { get; set; }
        /// <summary>
        /// Metadata contain implementation specific data
        /// </summary>
        public IGameItemMetadata Metadata { get; set; } = null;
        /// <summary>
        /// Properties of this item (<see cref="IItemPropertyProvider"/>)
        /// </summary>
        public ItemProperties Properties { get; set; } = null;

        /// <summary>
        /// Creates new instance of GameItem descriptor
        /// </summary>
        /// <param name="game"></param>
        /// <param name="symbol"></param>
        /// <param name="metadata"></param>
        public GameItem(GameID game, string symbol, IGameItemMetadata metadata)
        {
            Game = game;
            Symbol = symbol;
            Metadata = metadata;
        }
    }
}
