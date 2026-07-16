namespace BackendTest.Infrastructure.Models;

public class PersonPersistenceModel : IPersistenceModel
{
	public long Id { get; init; }

	public long CreatedAt { get; }

	public long UpdatedAt { get; }

	public string FirstName { get; init; }

	public string LastName { get; init; }

	public DateOnly YearOfBirth { get; init; }
}
