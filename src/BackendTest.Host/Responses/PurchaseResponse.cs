using System.Text.Json.Serialization;

namespace BackendTest.Host.Responses;

public sealed class PurchaseResponse
{
	[JsonPropertyName("id")]
	public long Id { get; init; }

	[JsonPropertyName("customer")]
	public PersonResponse Customer { get; init; }

	[JsonPropertyName("products")]
	public IEnumerable<ProductResponse> Products { get; init; }
}
