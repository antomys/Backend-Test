using BackendTest.Application.Mappers;
using BackendTest.Application.Models;
using BackendTest.Infrastructure.Context;
using BackendTest.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendTest.Application.Services;

/// <inheritdoc />
internal sealed class PersonService(
	IDbContextFactory<BackendTestDbContext> factory,
	ILogger<EntityService<PersonPersistenceModel, PersonApplicationModel>> logger)
	: EntityService<PersonPersistenceModel, PersonApplicationModel>(factory, logger, PersonMapper.Map, PersonMapper.Map);
