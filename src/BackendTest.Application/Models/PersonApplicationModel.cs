namespace BackendTest.Application.Models;

public sealed class PersonApplicationModel : IApplicationModel
{
	public long Id { get; init; }

	public string FirstName { get; init; }

	public string LastName { get; init; }

	public DateOnly YearOfBirth { get; init; }
}
