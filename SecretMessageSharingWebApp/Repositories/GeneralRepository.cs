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

	private CancellationToken GetToken(CancellationToken? ct) => ct ?? _cancellationTokenProvider.Token;

	public async Task<TEntity?> GetById(string id, CancellationToken? ct = null)
	{
		return await _dbSet.FindAsync(new object?[] { id }, GetToken(ct));
	}

	public async Task<int> Insert(TEntity entity, CancellationToken? ct = null)
	{
		await _dbSet.AddAsync(entity, GetToken(ct));
		return await _dbContext.SaveChangesAsync(GetToken(ct));
	}

	public async Task<int> Delete(TEntity entity, CancellationToken? ct = null)
	{
		_dbContext.Entry(entity).State = EntityState.Deleted;
		return await _dbContext.SaveChangesAsync(GetToken(ct));
	}

	public async Task<ICollection<TEntity>> SelectEntitiesWhere(Expression<Func<TEntity, bool>> predicate, CancellationToken? ct = null)
	{
		return await _dbSet
			.AsNoTracking()
			.Where(predicate)
			.ToListAsync(GetToken(ct));
	}

	public async Task<int> DeleteRangeBasedOnPredicate(Expression<Func<TEntity, bool>> predicate, CancellationToken? ct = null)
	{
		var token = GetToken(ct);
		
		var entitiesToDelete = await _dbSet
			.Where(predicate)
			.ToListAsync(token);

		if (entitiesToDelete.Count == 0)
		{
			return 0;
		}

		_dbSet.RemoveRange(entitiesToDelete);
		
		return await _dbContext.SaveChangesAsync(token);
	}
}