namespace BackendTest.Application.Models;

public sealed class PurchaseApplicationModel : IApplicationModel
{
	public long Id { get; init; }

	public PersonApplicationModel Customer { get; init; }

	public ICollection<ProductApplicationModel> Products { get; init; }
}
