using BackendTest.Application.Mappers;
using BackendTest.Application.Models;
using BackendTest.Infrastructure.Context;
using BackendTest.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendTest.Application.Services;

/// <inheritdoc />
internal sealed class PurchaseService(
	IDbContextFactory<BackendTestDbContext> factory,
	ILogger<EntityService<PurchasePersistenceModel, PurchaseApplicationModel>> logger)
	: EntityService<PurchasePersistenceModel, PurchaseApplicationModel>(
		factory,
		logger,
		PurchaseMapper.Map,
		PurchaseMapper.Map,
		static query => query
			.Include(static purchase => purchase.Customer)
			.Include(static purchase => purchase.Products))
{
	// The base Insert/Update rely on plain object graphs, but Add() would try to re-insert the
	// referenced products. Products must be resolved as already-tracked entities instead.
	public override async ValueTask<long?> Insert(PurchaseApplicationModel model, CancellationToken token)
	{
		ArgumentNullException.ThrowIfNull(model);

		var context = await factory.CreateDbContextAsync(token);
		try
		{
			var persistenceModel = model.Map()!;
			persistenceModel.Products.Clear();

			foreach (var productId in model.Products.Select(static product => product.Id))
			{
				persistenceModel.Products.Add(AttachProduct(context, productId));
			}

			await context.Set<PurchasePersistenceModel>().AddAsync(persistenceModel, token);

			var result = await context.SaveChangesAsync(token);

			return result > 0 ? persistenceModel.Id : null;
		}
		catch (DbUpdateException ex)
		{
			logger.LogWarning(ex, "Error while creating purchase for customer {CustomerId}.", model.Customer.Id);

			return null;
		}
		finally
		{
			await context.DisposeAsync();
		}
	}

	public override async ValueTask<bool> Update(PurchaseApplicationModel model, CancellationToken token)
	{
		ArgumentNullException.ThrowIfNull(model);

		var context = await factory.CreateDbContextAsync(token);
		try
		{
			var entity = await context.Set<PurchasePersistenceModel>()
				.Include(purchase => purchase.Products)
				.SingleOrDefaultAsync(purchase => purchase.Id == model.Id, token);

			if (entity is null)
			{
				logger.LogWarning("Purchase {Id} not found.", model.Id);

				return false;
			}

			context.Entry(entity).Property(static purchase => purchase.CustomerId).CurrentValue = model.Customer.Id;

			entity.Products.Clear();
			foreach (var productId in model.Products.Select(static product => product.Id))
			{
				entity.Products.Add(AttachProduct(context, productId));
			}

			return await context.SaveChangesAsync(token) > 0;
		}
		catch (DbUpdateException ex)
		{
			logger.LogWarning(ex, "Error while updating purchase {Id}.", model.Id);

			return false;
		}
		finally
		{
			await context.DisposeAsync();
		}
	}

	// Attaches a stub instead of querying the database; the FK constraint on save
	// catches a bogus product id via the DbUpdateException handling above.
	private static ProductPersistenceModel AttachProduct(BackendTestDbContext context, long productId)
	{
		var tracked = context.ChangeTracker.Entries<ProductPersistenceModel>()
			.Select(static entry => entry.Entity)
			.FirstOrDefault(product => product.Id == productId);

		if (tracked is not null)
		{
			return tracked;
		}

		var product = new ProductPersistenceModel { Id = productId };
		context.Attach(product);

		return product;
	}
}
