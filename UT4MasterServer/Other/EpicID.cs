using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Helpers;

namespace UT4MasterServer.Other;

[Serializable]
public struct EpicID : IComparable<EpicID>, IEquatable<EpicID>, IConvertible, IBsonClassMapAttribute
{
	private static readonly Random r = new Random();

	// TODO: make EpicID store 16 raw bytes (or 2x ulong?) for better performance
	public string ID { get; set; }

	[BsonIgnore]
	public bool IsEmpty
	{
		get
		{
			return string.IsNullOrWhiteSpace(ID) || ID == Empty.ID;
		}
	}

	private EpicID(string id)
	{
		if (id.Length != 32)
			throw new ArgumentException("id needs to be 32 hexadecimal characters long");

		ID = id.ToLower();

		if (!ID.IsHexString())
		{
			throw new ArgumentException("id needs to be a hexadecimal string");
		}
	}

	public static EpicID FromString(string id)
	{
		return new EpicID(id);
	}

	public static EpicID GenerateNew()
	{
		// MongoDB driver seems to do a similar thing, except that
		// it uses custom randomness generation instead of Random.
		byte[] bytes = new byte[16];
		r.NextBytes(bytes);
		string? id = Convert.ToHexString(bytes).ToLower();

		return new EpicID(id);
	}

	public static EpicID Empty
	{
		get { return new EpicID("00000000000000000000000000000000"); }
	}

	public static bool operator ==(EpicID lhs, EpicID rhs)
	{
		return lhs.Equals(rhs);
	}
	public static bool operator !=(EpicID lhs, EpicID rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object? obj)
	{
		if (obj == null)
			return false;

		if (obj is not EpicID)
			return false;

		var objUserID = (EpicID)obj;

		return string.Equals(ID, objUserID.ID, StringComparison.OrdinalIgnoreCase);
	}

	public override int GetHashCode()
	{
		return ID.GetHashCode();
	}

	public override string ToString()
	{
		if (ID == null)
			return Empty.ToString();
		return ID;
	}

	public int CompareTo(EpicID other)
	{
		return ID.CompareTo(other.ID);
	}

	public bool Equals(EpicID other)
	{
		return ID.Equals(other.ID);
	}

	public TypeCode GetTypeCode()
	{
		return ID.GetTypeCode();
	}

	public bool ToBoolean(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToBoolean(provider);
	}

	public byte ToByte(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToByte(provider);
	}

	public char ToChar(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToChar(provider);
	}

	public DateTime ToDateTime(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToDateTime(provider);
	}

	public decimal ToDecimal(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToDecimal(provider);
	}

	public double ToDouble(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToDouble(provider);
	}

	public short ToInt16(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToInt16(provider);
	}

	public int ToInt32(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToInt32(provider);
	}

	public long ToInt64(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToInt64(provider);
	}

	public sbyte ToSByte(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToSByte(provider);
	}

	public float ToSingle(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToSingle(provider);
	}

	public string ToString(IFormatProvider? provider)
	{
		return ID.ToString(provider);
	}

	public object ToType(Type conversionType, IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToType(conversionType, provider);
	}

	public ushort ToUInt16(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToUInt16(provider);
	}

	public uint ToUInt32(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToUInt32(provider);
	}

	public ulong ToUInt64(IFormatProvider? provider)
	{
		return ((IConvertible)ID).ToUInt64(provider);
	}

	public void Apply(BsonClassMap classMap)
	{
		throw new NotImplementedException();
	}

	public static bool operator <(EpicID left, EpicID right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator <=(EpicID left, EpicID right)
	{
		return left.CompareTo(right) <= 0;
	}

	public static bool operator >(EpicID left, EpicID right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator >=(EpicID left, EpicID right)
	{
		return left.CompareTo(right) >= 0;
	}
}
