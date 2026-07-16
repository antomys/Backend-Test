using BackendTest.Application.Mappers;
using BackendTest.Application.Models;
using BackendTest.Infrastructure.Context;
using BackendTest.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendTest.Application.Services;

/// <inheritdoc />
internal sealed class ProductService(
	IDbContextFactory<BackendTestDbContext> factory,
	ILogger<EntityService<ProductPersistenceModel, ProductApplicationModel>> logger)
	: EntityService<ProductPersistenceModel, ProductApplicationModel>(factory, logger, ProductMapper.Map, ProductMapper.Map);
