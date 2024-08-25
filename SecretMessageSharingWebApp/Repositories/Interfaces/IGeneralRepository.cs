using System.Linq.Expressions;
using SecretMessageSharingWebApp.Data;

namespace SecretMessageSharingWebApp.Repositories.Interfaces;

public interface IGeneralRepository<TEntity> where TEntity : class, IDbEntity
{
	Task<TEntity?> GetById(string id, CancellationToken? ct = null);

	Task<int> Insert(TEntity entity, CancellationToken? ct = null);

	Task<int> Delete(TEntity entity, CancellationToken? ct = null);

	Task<ICollection<TEntity>> SelectEntitiesWhere(Expression<Func<TEntity, bool>> predicate, CancellationToken? ct = null);

	Task<int> DeleteRangeBasedOnPredicate(Expression<Func<TEntity, bool>> predicate, CancellationToken? ct = null);
}
