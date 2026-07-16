namespace BackendTest.Infrastructure.Models;

public interface IPersistenceModel
{
	long Id { get; }

	long CreatedAt { get; }

	long UpdatedAt { get; }
}
