using System.Linq.Expressions;

namespace SecretMessageSharingWebApp.Repositories.Interfaces;

public interface IGeneralRepository<TEntity> where TEntity : class
{
	Task<TEntity?> Get(string id);

	Task Insert(TEntity entity, bool save = true);

	Task Delete(TEntity entity, bool save = true);

	Task<int> Save();

	IQueryable<TEntity> GetDbSetAsQueryable();

	Task<int> DeleteRangeBasedOnPredicate(Expression<Func<TEntity, bool>> predicate);
}
