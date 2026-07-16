namespace BackendTest.Infrastructure.Models;

public class ProductPersistenceModel : IPersistenceModel
{
	public long Id { get; init; }

	public long CreatedAt { get; }

	public long UpdatedAt { get; }

	public string Name { get; init; }

	public string Type { get; init; }

	public double Price { get; init; }
}
