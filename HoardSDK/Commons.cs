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
        System.Numerics.BigInteger Value;

        public HoardID(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value cannot be an empty string!");
            if (value.StartsWith("0x"))
                value = value.Substring(2);

            System.Numerics.BigInteger bigValue = System.Numerics.BigInteger.Parse(value, System.Globalization.NumberStyles.AllowHexSpecifier);

            //validity check
            System.Numerics.BigInteger maxUValue = System.Numerics.BigInteger.Pow(2, 160) - 1;

            if (bigValue > maxUValue) throw new ArgumentOutOfRangeException(nameof(value),
                $"HoardID integer must not exceed maximum value for int20: {maxUValue}. Current value is: {value}");

            Value = bigValue;            
        }

        public HoardID(System.Numerics.BigInteger value)
        {
            //validity check
            System.Numerics.BigInteger maxUValue = System.Numerics.BigInteger.Pow(2, 160) - 1;

            if (value > maxUValue) throw new ArgumentOutOfRangeException(nameof(value),
                $"HoardID integer must not exceed maximum value for int20: {maxUValue}. Current value is: {value}");

            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString("x");
        }

        public byte[] ToHexByteArray()
        {
            if (BitConverter.IsLittleEndian)
                return Value.ToByteArray().Reverse().ToArray();
            return Value.ToByteArray();
        }

        public static implicit operator string(HoardID addr)
        {
            return addr.Value.ToString("x");
        }

        public static implicit operator System.Numerics.BigInteger(HoardID addr)
        {
            return addr.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is HoardID)
                return ((HoardID)obj).Value == Value;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    /// <summary>
    /// User account basic information.
    /// </summary>
    public abstract class AccountInfo
    {
        public HoardID ID { get; private set; } = null;
        public string Name { get; private set; } = null;

        public AccountInfo(string name, HoardID id)
        {
            Name = name;
            ID = id;
        }

        public abstract Task<string> SignTransaction(byte[] input);

        public abstract Task<string> SignMessage(byte[] input);

        public abstract Task<AccountInfo> Activate(User user);
    }

    /// <summary>
    /// Hoard user.
    /// </summary>
    public class User
    {
        public string UserName { get; private set; } = "";
        public string HoardId { get; internal set; } = "";
        public List<AccountInfo> Accounts = new List<AccountInfo>();
        public AccountInfo ActiveAccount { get; private set; } = null;

        public User(string name)
        {
            UserName = name;
        }

        public bool ChangeActiveAccount(AccountInfo account)
        {
            ActiveAccount = account.Activate(this).Result;
            return ActiveAccount != null;
        }
    }

    class BigIntegerHexConverter : System.ComponentModel.TypeConverter
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
        [System.ComponentModel.TypeConverter(typeof(BigIntegerHexConverter))]
        public BigInteger ID { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string GameOwner { get; set; }

        public static GameID kInvalidID { get; private set; } = new GameID(BigInteger.Zero);

        public GameID(BigInteger id)
        {
            System.Diagnostics.Trace.Assert(id < (BigInteger.One << 256));
            ID = id;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public static GameID FromName(string name)
        {
            var sha3 = new Org.BouncyCastle.Crypto.Digests.KeccakDigest(256);
            byte[] hashb = new byte[sha3.GetDigestSize()];
            byte[] value = System.Text.Encoding.UTF8.GetBytes(name);
            sha3.Reset();
            sha3.BlockUpdate(value, 0, value.Length);
            sha3.DoFinal(hashb, 0);
            var v = new System.Numerics.BigInteger(hashb.Reverse().ToArray());
            if (v.Sign<0)
            {
                v = v + (BigInteger.One << 256);
            }
            return new GameID(v);
        }

        public bool Equals(GameID other)
        {
            if (other == null)
                return false;

            if (ID == other.ID)
                return true;
            else
                return false;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            GameID gameObj = obj as GameID;
            if (gameObj == null)
                return false;
            else
                return Equals(gameObj);
        }

        public override string ToString()
        {
            return ID.ToString("x");
        }

        public static bool operator ==(GameID game1, GameID game2)
        {
            if (((object)game1) == null || ((object)game2) == null)
                return object.Equals(game1, game2);

            return game1.Equals(game2);
        }

        public static bool operator !=(GameID game1, GameID game2)
        {
            if (((object)game1) == null || ((object)game2) == null)
                return !object.Equals(game1, game2);

            return !(game1.Equals(game2));
        }
    }
}
