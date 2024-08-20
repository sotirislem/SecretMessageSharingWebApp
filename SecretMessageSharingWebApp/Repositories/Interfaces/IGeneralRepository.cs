using System.Linq.Expressions;
using SecretMessageSharingWebApp.Data;

namespace SecretMessageSharingWebApp.Repositories.Interfaces;

public interface IGeneralRepository<TEntity> where TEntity : class, IDbEntity
{
	Task<TEntity?> GetById(string id);

	Task<int> Insert(TEntity entity);

	Task<int> Delete(TEntity entity);

	Task<ICollection<TEntity>> SelectEntitiesWhere(Expression<Func<TEntity, bool>> predicate);

	Task<int> DeleteRangeBasedOnPredicate(Expression<Func<TEntity, bool>> predicate);
}
