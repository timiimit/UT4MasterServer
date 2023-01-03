using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace UT4MasterServer.Other;

/// <summary>
/// Represents a serializer for Strings.
/// </summary>
public class EpicIDSerializer : StructSerializerBase<EpicID>, IRepresentationConfigurable<EpicIDSerializer>
{
	#region static
	private static readonly StringSerializer __instance = new StringSerializer();

	// public static properties
	/// <summary>
	/// Gets a cached instance of a default string serializer.
	/// </summary>
	public static StringSerializer Instance => __instance;
	#endregion

	// private fields
	private readonly BsonType _representation;

	// constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="StringSerializer"/> class.
	/// </summary>
	public EpicIDSerializer() : this(BsonType.String)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="StringSerializer"/> class.
	/// </summary>
	/// <param name="representation">The representation.</param>
	public EpicIDSerializer(BsonType representation)
	{
		switch (representation)
		{
			case BsonType.ObjectId:
			case BsonType.String:
			case BsonType.Symbol:
				break;

			default:
				var message = string.Format("{0} is not a valid representation for a EpicIDSerializer.", representation);
				throw new ArgumentException(message);
		}

		_representation = representation;
	}

	// public properties
	/// <summary>
	/// Gets the representation.
	/// </summary>
	/// <value>
	/// The representation.
	/// </value>
	public BsonType Representation
	{
		get { return _representation; }
	}

	// public methods
	/// <summary>
	/// Deserializes a value.
	/// </summary>
	/// <param name="context">The deserialization context.</param>
	/// <param name="args">The deserialization args.</param>
	/// <returns>A deserialized value.</returns>
	public override EpicID Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
	{
		var bsonReader = context.Reader;

		var bsonType = bsonReader.GetCurrentBsonType();
		switch (bsonType)
		{
			case BsonType.ObjectId:
				if (_representation == BsonType.ObjectId)
				{
					return EpicID.FromString(bsonReader.ReadObjectId().ToString());
				}

				goto default;

			case BsonType.String:
				return EpicID.FromString(bsonReader.ReadString());

			case BsonType.Symbol:
				return EpicID.FromString(bsonReader.ReadSymbol());

			default:
				throw CreateCannotDeserializeFromBsonTypeException(bsonType);
		}
	}

	/// <summary>
	/// Serializes a value.
	/// </summary>
	/// <param name="context">The serialization context.</param>
	/// <param name="args">The serialization args.</param>
	/// <param name="value">The object.</param>
	public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, EpicID value)
	{
		var bsonWriter = context.Writer;

		switch (_representation)
		{
			case BsonType.ObjectId:
				bsonWriter.WriteObjectId(ObjectId.Parse(value.ID));
				break;

			case BsonType.String:
				bsonWriter.WriteString(value.ID);
				break;

			case BsonType.Symbol:
				bsonWriter.WriteSymbol(value.ID);
				break;

			default:
				var message = string.Format("'{0}' is not a valid String representation.", _representation);
				throw new BsonSerializationException(message);
		}
	}

	/// <summary>
	/// Returns a serializer that has been reconfigured with the specified representation.
	/// </summary>
	/// <param name="representation">The representation.</param>
	/// <returns>The reconfigured serializer.</returns>
	public EpicIDSerializer WithRepresentation(BsonType representation)
	{
		if (representation == _representation)
		{
			return this;
		}
		else
		{
			return new EpicIDSerializer(representation);
		}
	}

	// explicit interface implementations
	IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
	{
		return WithRepresentation(representation);
	}
}
