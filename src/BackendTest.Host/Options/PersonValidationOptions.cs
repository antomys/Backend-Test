using System.ComponentModel.DataAnnotations;

namespace BackendTest.Host.Options;

public sealed class PersonValidationOptions
{
	public const string SectionName = "Validation:Person";

	[Required(AllowEmptyStrings = false)]
	public int MinimumAge { get; init; }
}
