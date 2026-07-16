using System.Text.Json.Serialization;

namespace BackendTest.Host.Requests;

/// <summary>
/// Payload for creating a purchase.
/// </summary>
public sealed class PurchaseAddRequest
{
	[JsonPropertyName("customerId")]
	public long CustomerId { get; init; }

	[JsonPropertyName("productIds")]
	public IReadOnlyCollection<long> ProductIds { get; init; }
}
