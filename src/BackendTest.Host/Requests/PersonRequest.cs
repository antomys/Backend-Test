using System.Text.Json.Serialization;

namespace BackendTest.Host.Requests;

/// <summary>
/// Payload for creating a person.
/// </summary>
public sealed class PersonRequest
{
	[JsonPropertyName("id")]
	public long Id { get; init; }

	[JsonPropertyName("firstName")]
	public string FirstName { get; init; }

	[JsonPropertyName("lastName")]
	public string LastName { get; init; }

	[JsonPropertyName("yearOfBirth")]
	public DateOnly YearOfBirth { get; init; }
}
