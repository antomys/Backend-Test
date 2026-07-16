using System.Text.Json.Serialization;

namespace BackendTest.Host.Requests;

/// <summary>
/// Payload for creating a person.
/// </summary>
public sealed class ProductRequest
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
