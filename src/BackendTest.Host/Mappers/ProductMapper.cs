using BackendTest.Application.Models;
using BackendTest.Host.Requests;
using BackendTest.Host.Responses;

namespace BackendTest.Host.Mappers;

internal static class ProductMapper
{
	public static ProductApplicationModel? Map(
		this ProductAddRequest? model)
	{
		if (model is null)
		{
			return null;
		}

		return new ProductApplicationModel
		{
			Name = model.Name,
			Type = model.Type,
			Price = model.Price,
		};
	}

	public static ProductApplicationModel? Map(
		this ProductRequest? model)
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

	public static ProductResponse? Map(
		this ProductApplicationModel? model)
	{
		if (model is null)
		{
			return null;
		}

		return new ProductResponse
		{
			Id = model.Id,
			Name = model.Name,
			Type = model.Type,
			Price = model.Price,
		};
	}
}
