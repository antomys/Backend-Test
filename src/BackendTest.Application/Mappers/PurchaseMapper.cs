using BackendTest.Application.Models;
using BackendTest.Infrastructure.Models;

namespace BackendTest.Application.Mappers;

internal static class PurchaseMapper
{
	public static PurchaseApplicationModel? Map(
		this PurchasePersistenceModel? model)
	{
		if (model is null)
		{
			return null;
		}

		return new PurchaseApplicationModel
		{
			Id = model.Id,
			Customer = model.Customer.Map(),
			Products = model.Products?.Select(static product => product.Map()).ToArray() ?? [],
		};
	}

	public static PurchasePersistenceModel? Map(
		this PurchaseApplicationModel? model)
	{
		if (model is null)
		{
			return null;
		}

		return new PurchasePersistenceModel
		{
			Id = model.Id,
			CustomerId = model.Customer.Id,
			Products = model.Products.Select(static product => product.Map()).ToList(),
		};
	}
}
