using System.Text.Json.Serialization;

namespace BackendTest.Host.Responses;

public sealed class ProductResponse
{
	[JsonPropertyName("id")]
	public long Id { get; init; }

	[JsonPropertyName("name")]
	public string Name { get; init; }

	[JsonPropertyName("type")]
	public string Type { get; init; }

	[JsonPropertyName("price")]
	public double Price { get; init; }
}
