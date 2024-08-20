using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services.Interfaces;
using System.Linq.Expressions;

namespace SecretMessageSharingWebApp.Repositories;

public class GeneralRepository<TEntity> : IGeneralRepository<TEntity> where TEntity : class, IDbEntity
{
	protected readonly IDateTimeProviderService _dateTimeProviderService;

	protected readonly SecretMessagesDbContext _dbContext;
	protected readonly DbSet<TEntity> _dbSet;

	public GeneralRepository(SecretMessagesDbContext context, IDateTimeProviderService dateTimeProviderService)
	{
		_dateTimeProviderService = dateTimeProviderService;

		_dbContext = context;
		_dbSet = context.Set<TEntity>();
	}

	public async Task<TEntity?> GetById(string id)
	{
		return await _dbSet.FindAsync(id);
	}

	public async Task<int> Insert(TEntity entity)
	{
		_dbSet.Add(entity);

		return await _dbContext.SaveChangesAsync();
	}

	public async Task<int> Delete(TEntity entity)
	{
		if (_dbContext.Entry(entity).State == EntityState.Detached)
		{
			_dbSet.Attach(entity);
		}

		_dbSet.Remove(entity);

		return await _dbContext.SaveChangesAsync();
	}

	public async Task<ICollection<TEntity>> SelectEntitiesWhere(Expression<Func<TEntity, bool>> predicate)
	{
		return await _dbSet
			.Where(predicate)
			.AsNoTracking()
			.ToListAsync();
	}

	public async Task<int> DeleteRangeBasedOnPredicate(Expression<Func<TEntity, bool>> predicate)
	{
		var results = _dbSet.Where(predicate);
		var resultsCount = await results.CountAsync();

		if (resultsCount > 0)
		{
			_dbSet.RemoveRange(results);

			return await _dbContext.SaveChangesAsync();
		}

		return 0;
	}
}
