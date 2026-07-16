using System.Runtime.CompilerServices;
using BackendTest.Application.Interfaces;
using BackendTest.Application.Models;
using BackendTest.Infrastructure.Context;
using BackendTest.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendTest.Application.Services;

/// <inheritdoc />
internal class EntityService<TPersistenceModel, TApplicationModel>(
	IDbContextFactory<BackendTestDbContext> factory,
	ILogger<EntityService<TPersistenceModel, TApplicationModel>> logger,
	Func<TPersistenceModel?, TApplicationModel?> toApplicationModel,
	Func<TApplicationModel?, TPersistenceModel?> toPersistenceModel,
	Func<IQueryable<TPersistenceModel>, IQueryable<TPersistenceModel>>? includeRelated = null) : IEntityService<TApplicationModel>
	where TPersistenceModel : class, IPersistenceModel
	where TApplicationModel : class, IApplicationModel
{
	public async IAsyncEnumerable<TApplicationModel?> GetAll([EnumeratorCancellation] CancellationToken token)
	{
		var context = await factory.CreateDbContextAsync(token);
		try
		{
			var query = context.Set<TPersistenceModel>().AsNoTracking();
			var entities = (includeRelated?.Invoke(query) ?? query).ToAsyncEnumerable();

			await foreach (var entity in entities.WithCancellation(token))
			{
				yield return toApplicationModel(entity);
			}
		}
		finally
		{
			await context.DisposeAsync();
		}
	}

	public async ValueTask<TApplicationModel?> Get(long id, CancellationToken token)
	{
		if (id is 0)
		{
			throw new ArgumentNullException(nameof(id));
		}

		var context = await factory.CreateDbContextAsync(token);
		try
		{
			var query = context.Set<TPersistenceModel>().AsNoTracking();
			var entity = await (includeRelated?.Invoke(query) ?? query)
				.SingleOrDefaultAsync(model => model.Id == id, token);

			return toApplicationModel(entity);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error while getting {Entity}.", typeof(TPersistenceModel).Name);

			return null;
		}
		finally
		{
			await context.DisposeAsync();
		}
	}

	public virtual async ValueTask<long?> Insert(TApplicationModel model, CancellationToken token)
	{
		ArgumentNullException.ThrowIfNull(model);

		var context = await factory.CreateDbContextAsync(token);
		try
		{
			var persistenceModel = toPersistenceModel(model)!;
			await context.Set<TPersistenceModel>().AddAsync(persistenceModel, token);

			var result = await context.SaveChangesAsync(token);

			if (result > 0)
			{
				return persistenceModel.Id;
			}
		}
		catch (DbUpdateException ex)
		{
			logger.LogWarning(ex, "A {Entity} with id {Id} already exists.", typeof(TPersistenceModel).Name, model.Id);

			return null;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error while creating {Entity} with id {Id}.", typeof(TPersistenceModel).Name, model.Id);

			return null;
		}
		finally
		{
			await context.DisposeAsync();
		}

		return null;
	}

	public virtual async ValueTask<bool> Update(TApplicationModel model, CancellationToken token)
	{
		ArgumentNullException.ThrowIfNull(model);

		var context = await factory.CreateDbContextAsync(token);
		try
		{
			var entity = await context.Set<TPersistenceModel>().SingleOrDefaultAsync(m => m.Id == model.Id, token);

			if (entity is null)
			{
				logger.LogWarning("{Entity} {Id} not found.", typeof(TPersistenceModel).Name, model.Id);

				return false;
			}

			var persistenceModel = toPersistenceModel(model)!;
			context.Entry(entity).CurrentValues.SetValues(persistenceModel);

			return await context.SaveChangesAsync(token) > 0;
		}
		catch (DbUpdateException ex)
		{
			logger.LogWarning(ex, "Error while updating {Entity} with id {Id}.", typeof(TPersistenceModel).Name, model.Id);

			return false;
		}
		finally
		{
			await context.DisposeAsync();
		}
	}

	public async ValueTask<bool> Delete(long id, CancellationToken token)
	{
		if (id is 0)
		{
			throw new ArgumentNullException(nameof(id));
		}

		var context = await factory.CreateDbContextAsync(token);

		try
		{
			var entity = await context.Set<TPersistenceModel>().SingleOrDefaultAsync(model => model.Id == id, token);

			if (entity is null)
			{
				logger.LogWarning("{Entity} {Id} not found.", typeof(TPersistenceModel).Name, id);

				return false;
			}

			context.Set<TPersistenceModel>().Remove(entity);

			return await context.SaveChangesAsync(token) > 0;
		}
		finally
		{
			await context.DisposeAsync();
		}
	}
}
