using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackendTest.Host;

public sealed class ExceptionJsonConverter : JsonConverter<Exception>
{
	public static readonly JsonSerializerOptions WithSafeException = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new ExceptionJsonConverter(), new JsonStringEnumConverter() },
		NumberHandling = JsonNumberHandling.AllowReadingFromString,
	};

	public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotSupportedException("Deserializing exceptions is not supported.");
	}

	public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteString("type", value.GetType().FullName);
		writer.WriteString("message", value.Message);

		writer.WriteEndObject();
	}
}
