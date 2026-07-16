using BackendTest.Application.Models;
using BackendTest.Host.Requests;
using BackendTest.Host.Responses;

namespace BackendTest.Host.Mappers;

internal static class PersonMapper
{
	public static PersonApplicationModel? Map(
		this PersonAddRequest? model)
	{
		if (model is null)
		{
			return null;
		}

		return new PersonApplicationModel
		{
			FirstName = model.FirstName,
			LastName = model.LastName,
			YearOfBirth = model.YearOfBirth,
		};
	}

	public static PersonApplicationModel? Map(
		this PersonRequest? model)
	{
		if (model is null)
		{
			return null;
		}

		return new PersonApplicationModel
		{
			Id = model.Id,
			FirstName = model.FirstName,
			LastName = model.LastName,
			YearOfBirth = model.YearOfBirth,
		};
	}

	public static PersonResponse? Map(
		this PersonApplicationModel? model)
	{
		if (model is null)
		{
			return null;
		}

		return new PersonResponse
		{
			Id = model.Id,
			FirstName = model.FirstName,
			LastName = model.LastName,
			YearOfBirth = model.YearOfBirth,
		};
	}
}
