using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Repositories;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using System.Linq.Expressions;

namespace SecretMessageSharingWebApp.Repositories
{
	public class GeneralRepository<TEntity> : IGeneralRepository<TEntity> where TEntity : class
	{
		protected readonly SecretMessagesDbContext _context;
		protected readonly DbSet<TEntity> _dbSet;

		public GeneralRepository(SecretMessagesDbContext context)
		{
			_context = context;
			_dbSet = context.Set<TEntity>();
		}

		public async Task<TEntity?> Get(string id)
		{
			return await _dbSet.FindAsync(id);
		}

		public async Task Insert(TEntity entity, bool save = true)
		{
			_dbSet.Add(entity);
			if (save) await Save();
		}

		public async Task Delete(TEntity entity, bool save = true)
		{
			if (_context.Entry(entity).State == EntityState.Detached)
			{
				_dbSet.Attach(entity);
			}

			_dbSet.Remove(entity);
			if (save) await Save();
		}

		public async Task<int> Save()
		{
			return await _context.SaveChangesAsync();
		}

		public IQueryable<TEntity> GetDbSetAsQueryable()
		{
			return _dbSet.AsNoTracking().AsQueryable();
		}

		public async Task<int> DeleteRangeBasedOnPredicate(Expression<Func<TEntity, bool>> predicate)
		{
			var results = _dbSet.Where(predicate);
			if (results.Count() > 0)
			{
				_dbSet.RemoveRange(results);
				var dbSaveResult = await Save();

				return dbSaveResult;
			}

			return 0;
		}
	}
}
