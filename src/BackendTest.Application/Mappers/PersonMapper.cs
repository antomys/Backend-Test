using BackendTest.Application.Models;
using BackendTest.Infrastructure.Models;

namespace BackendTest.Application.Mappers;

internal static class PersonMapper
{
	public static PersonApplicationModel? Map(
		this PersonPersistenceModel? model)
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

	public static PersonPersistenceModel? Map(
		this PersonApplicationModel? model)
	{
		if (model is null)
		{
			return null;
		}

		return new PersonPersistenceModel
		{
			Id = model.Id,
			FirstName = model.FirstName,
			LastName = model.LastName,
			YearOfBirth = model.YearOfBirth,
		};
	}
}
