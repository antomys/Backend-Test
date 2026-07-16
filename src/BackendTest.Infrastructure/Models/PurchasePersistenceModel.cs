namespace BackendTest.Infrastructure.Models;

public class PurchasePersistenceModel : IPersistenceModel
{
	public long Id { get; init; }

	public long CreatedAt { get; }

	public long UpdatedAt { get; }

	public long CustomerId { get; init; }

	public virtual PersonPersistenceModel Customer { get; init; }

	public virtual ICollection<ProductPersistenceModel> Products { get; init; }
}
