using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Hoard identifier (20 byte integer).
    /// </summary>
    public class HoardID
    {
        private BigInteger Value;

        /// <summary>
        /// Constructor from string
        /// </summary>
        /// <param name="value">a properly formed string representing a 160bit big integer </param>
        public HoardID(string value)
        {
            if (!Nethereum.Util.AddressUtil.Current.IsValidEthereumAddressHexFormat(value))
                throw new ArgumentException($"Value {value} is not a valid address!");
            if (value.StartsWith("0x"))
                value = value.Substring(2);

            // ensure id is always positive integer
            BigInteger bigValue = BigInteger.Parse("00" + value, System.Globalization.NumberStyles.AllowHexSpecifier);

            //validity check
            BigInteger maxUValue = BigInteger.Pow(2, 160) - 1;

            if (bigValue > maxUValue) throw new ArgumentOutOfRangeException(nameof(value),
                $"HoardID integer must not exceed maximum value for int20: {maxUValue}. Current value is: {value}");

            Value = bigValue;            
        }

        /// <summary>
        /// Constructor from BigInteger
        /// </summary>
        /// <param name="value">a proper 160bit value</param>
        public HoardID(BigInteger value)
        {
            //validity check
            BigInteger maxUValue = BigInteger.Pow(2, 160) - 1;

            if (value > maxUValue) throw new ArgumentOutOfRangeException(nameof(value),
                $"HoardID integer must not exceed maximum value for int20: {maxUValue}. Current value is: {value}");

            Value = value;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var ret = Value.ToString("x40");
            return "0x"+ret.Substring(ret.Length - 40);
        }

        /// <summary>
        /// Returns hex byte representation of this object
        /// </summary>
        /// <returns></returns>
        public byte[] ToHexByteArray()
        {
            if (BitConverter.IsLittleEndian)
                return Value.ToByteArray().Reverse().ToArray();
            return Value.ToByteArray();
        }

        /// <summary>
        /// Default cast string operator
        /// </summary>
        /// <param name="addr"></param>
        public static implicit operator string(HoardID addr)
        {
            return addr.ToString();
        }

        /// <summary>
        /// Defulat cast operator to BigInteger
        /// </summary>
        /// <param name="addr"></param>
        public static implicit operator BigInteger(HoardID addr)
        {
            return addr.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addr1"></param>
        /// <param name="addr2"></param>
        /// <returns></returns>
        public static bool operator ==(HoardID addr1, HoardID addr2)
        {
            return (addr1?.ToString() == addr2?.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addr1"></param>
        /// <param name="addr2"></param>
        /// <returns></returns>
        public static bool operator !=(HoardID addr1, HoardID addr2)
        {
            return (addr1?.ToString() != addr2?.ToString());
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is HoardID)
                return ((HoardID)obj).Value == Value;
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    /// <summary>
    /// User account basic information.
    /// </summary>
    public abstract class Profile
    {
        /// <summary>
        /// HoardID of user profile (public address)
        /// </summary>
        public HoardID ID { get; private set; } = null;

        /// <summary>
        /// Name of user profile (for convenience)
        /// </summary>
        public string Name { get; private set; } = null;

        /// <summary>
        /// Basic constructor of user profile
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="id">Identifier (public address)</param>
        public Profile(string name, HoardID id)
        {
            Name = name;
            ID = id;
        }

        /// <summary>
        /// Sign any transaction with user profile
        /// </summary>
        /// <param name="input">input arguments</param>
        /// <returns>signed transaction string</returns>
        public abstract Task<string> SignTransaction(byte[] input);

        /// <summary>
        /// Sign any message with user profile
        /// </summary>
        /// <param name="input">input arguments</param>
        /// <returns>signed message</returns>
        public abstract Task<string> SignMessage(byte[] input);
    }

    internal class BigIntegerHexConverter : System.ComponentModel.TypeConverter
    {
        public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return ((BigInteger)value).ToString("x");
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    /// <summary>
    /// Game identifier.
    /// </summary>
    public class GameID : IEquatable<GameID>
    {
        /// <summary>
        /// Unique ID of the game (256 hash made from name)
        /// </summary>
        [System.ComponentModel.TypeConverter(typeof(BigIntegerHexConverter))]
        public BigInteger ID { get; set; }
        /// <summary>
        /// User friendly name of the game
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Unique name identifier of the game
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// Url of game server
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Owner of the game
        /// </summary>
        public string GameOwner { get; set; }

        /// <summary>
        /// Invalid value
        /// </summary>
        public static GameID kInvalidID { get; private set; } = new GameID(BigInteger.Zero);

        /// <summary>
        /// Default constructor from 256 big integer
        /// </summary>
        /// <param name="id">256 big integer</param>
        public GameID(BigInteger id)
        {
            System.Diagnostics.Trace.Assert(id < (BigInteger.One << 256));
            ID = id;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        /// <summary>
        /// Creates a proper GameID from name (calculates proper ID)
        /// </summary>
        /// <param name="symbol">Unique name identifier of the game</param>
        /// <returns>GameID object</returns>
        public static GameID FromName(string symbol)
        {
            var sha3 = new Org.BouncyCastle.Crypto.Digests.KeccakDigest(256);
            byte[] hashb = new byte[sha3.GetDigestSize()];
            byte[] value = System.Text.Encoding.UTF8.GetBytes(symbol);
            sha3.Reset();
            sha3.BlockUpdate(value, 0, value.Length);
            sha3.DoFinal(hashb, 0);
            var v = new System.Numerics.BigInteger(hashb.Reverse().ToArray());
            if (v.Sign<0)
            {
                v = v + (BigInteger.One << 256);//make it always positive
            }
            GameID game = new GameID(v);
            game.Symbol = symbol;
            return game;
        }

        /// <summary>
        /// Check if other is same (compares IDs)
        /// </summary>
        /// <param name="other">object to compare to</param>
        /// <returns>true if other has same ID, false otherwise</returns>
        public bool Equals(GameID other)
        {
            if (other == null)
                return false;

            if (ID == other.ID)
                return true;
            else
                return false;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            GameID gameObj = obj as GameID;
            if (gameObj == null)
                return false;
            else
                return Equals(gameObj);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ID.ToString("x");
        }

        /// <summary>
        /// Checks equality of two games
        /// </summary>
        /// <param name="game1"></param>
        /// <param name="game2"></param>
        /// <returns>true if both are the same, false otherwise</returns>
        public static bool operator ==(GameID game1, GameID game2)
        {
            if (((object)game1) == null || ((object)game2) == null)
                return Equals(game1, game2);

            return game1.Equals(game2);
        }

        /// <summary>
        /// Checks inequality of two games
        /// </summary>
        /// <param name="game1"></param>
        /// <param name="game2"></param>
        /// <returns>true if both are different, false otherwise</returns>
        public static bool operator !=(GameID game1, GameID game2)
        {
            if (((object)game1) == null || ((object)game2) == null)
                return !Equals(game1, game2);

            return !(game1.Equals(game2));
        }
    }

    /// <summary>
    /// Item type information used for queries.
    /// </summary>
    public class GameItemType
    {
        /// <summary>
        /// Name of the item type
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Symbol of item
        /// </summary>
        public string Symbol { get; private set; }
        /// <summary>
        /// Type of the item
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// Type of Item state
        /// </summary>
        public string StateType { get; private set; }
        /// <summary>
        /// Totoal available supply of items
        /// </summary>
        public BigInteger TotalSupply { get; private set; }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="symbol"></param>
        /// <param name="type"></param>
        public GameItemType(string name, string symbol, string type)
        {
            Name = name;
            Symbol = symbol;
            Type = type;
        }
    }
}
