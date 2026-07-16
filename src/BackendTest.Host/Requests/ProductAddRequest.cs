using System.Text.Json.Serialization;

namespace BackendTest.Host.Requests;

/// <summary>
/// Payload for creating a product.
/// </summary>
public sealed class ProductAddRequest
{
	[JsonPropertyName("name")]
	public string Name { get; init; }

	[JsonPropertyName("type")]
	public string Type { get; init; }

	[JsonPropertyName("price")]
	public double Price { get; init; }
}
