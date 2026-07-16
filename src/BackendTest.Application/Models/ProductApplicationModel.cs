namespace BackendTest.Application.Models;

public sealed class ProductApplicationModel : IApplicationModel
{
	public long Id { get; init; }

	public string Name { get; init; }

	public string Type { get; init; }

	public double Price { get; init; }
}
