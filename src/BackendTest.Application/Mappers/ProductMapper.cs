using BackendTest.Application.Models;
using BackendTest.Infrastructure.Models;

namespace BackendTest.Application.Mappers;

internal static class ProductMapper
{
	public static ProductApplicationModel? Map(
		this ProductPersistenceModel? model)
	{
		if (model is null)
		{
			return null;
		}

		return new ProductApplicationModel
		{
			Id = model.Id,
			Name = model.Name,
			Type = model.Type,
			Price = model.Price,
		};
	}

	public static ProductPersistenceModel? Map(
		this ProductApplicationModel? model)
	{
		if (model is null)
		{
			return null;
		}

		return new ProductPersistenceModel
		{
			Id = model.Id,
			Name = model.Name,
			Type = model.Type,
			Price = model.Price,
		};
	}
}
