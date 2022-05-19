using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Repositories.Interfaces;

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

		public virtual TEntity? Get(string id)
		{
			return _dbSet.Find(id);
		}

		public virtual IQueryable<TEntity> GetAll()
		{
			return _dbSet.AsQueryable();
		}

		public virtual void Insert(TEntity entity, bool save = false)
		{
			_dbSet.Add(entity);
			if (save) Save();
		}

		public virtual void Delete(TEntity entity, bool save = false)
		{
			if (_context.Entry(entity).State == EntityState.Detached)
			{
				_dbSet.Attach(entity);
			}

			_dbSet.Remove(entity);
			if (save) Save();
		}

		public int Save()
		{
			return _context.SaveChanges();
		}
	}
}
