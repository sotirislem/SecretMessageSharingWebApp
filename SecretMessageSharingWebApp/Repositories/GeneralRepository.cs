using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Providers;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services.Interfaces;
using System.Linq.Expressions;

namespace SecretMessageSharingWebApp.Repositories;

public class GeneralRepository<TEntity> : IGeneralRepository<TEntity> where TEntity : class, IDbEntity
{
	protected readonly IDateTimeProviderService _dateTimeProviderService;
	protected readonly ICancellationTokenProvider _cancellationTokenProvider;

	protected readonly SecretMessagesDbContext _dbContext;
	protected readonly DbSet<TEntity> _dbSet;

	public GeneralRepository(
		SecretMessagesDbContext context,
		IDateTimeProviderService dateTimeProviderService,
		ICancellationTokenProvider cancellationTokenProvider)
	{
		_dateTimeProviderService = dateTimeProviderService;
		_cancellationTokenProvider = cancellationTokenProvider;

		_dbContext = context;
		_dbSet = context.Set<TEntity>();
	}

	public async Task<TEntity?> GetById(string id, CancellationToken? ct = null)
	{
		ct ??= _cancellationTokenProvider.Token;

		return await _dbSet.FindAsync(new object?[] { id }, (CancellationToken)ct);
	}

	public async Task<int> Insert(TEntity entity, CancellationToken? ct = null)
	{
		ct ??= _cancellationTokenProvider.Token;

		_dbSet.Add(entity);

		return await _dbContext.SaveChangesAsync((CancellationToken)ct);
	}

	public async Task<int> Delete(TEntity entity, CancellationToken? ct = null)
	{
		ct ??= _cancellationTokenProvider.Token;

		if (_dbContext.Entry(entity).State == EntityState.Detached)
		{
			_dbSet.Attach(entity);
		}

		_dbSet.Remove(entity);

		return await _dbContext.SaveChangesAsync((CancellationToken)ct);
	}

	public async Task<ICollection<TEntity>> SelectEntitiesWhere(Expression<Func<TEntity, bool>> predicate, CancellationToken? ct = null)
	{
		ct ??= _cancellationTokenProvider.Token;

		return await _dbSet
			.Where(predicate)
			.AsNoTracking()
			.ToListAsync((CancellationToken)ct);
	}

	public async Task<int> DeleteRangeBasedOnPredicate(Expression<Func<TEntity, bool>> predicate, CancellationToken? ct = null)
	{
		ct ??= _cancellationTokenProvider.Token;

		var results = _dbSet.Where(predicate);
		var resultsCount = await results.CountAsync((CancellationToken)ct);

		if (resultsCount > 0)
		{
			_dbSet.RemoveRange(results);

			return await _dbContext.SaveChangesAsync((CancellationToken)ct);
		}

		return 0;
	}
}
