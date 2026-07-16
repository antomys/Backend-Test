using BackendTest.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BackendTest.Infrastructure;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default)
	{
		var entries = eventData.Context?.ChangeTracker.Entries<IPersistenceModel>().ToArray();

		if (entries?.Length is null or 0)
		{
			return base.SavingChangesAsync(eventData, result, cancellationToken);
		}

		var updateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

		foreach (ref var entry in entries.AsSpan())
		{
			if (entry.State is EntityState.Modified or EntityState.Added)
			{
				entry.Property(nameof(IPersistenceModel.UpdatedAt)).CurrentValue = updateTime;
			}

			if (entry.State is EntityState.Added)
			{
				entry.Property(nameof(IPersistenceModel.CreatedAt)).CurrentValue = updateTime;
			}
		}

		return base.SavingChangesAsync(eventData, result, cancellationToken);
	}
}
