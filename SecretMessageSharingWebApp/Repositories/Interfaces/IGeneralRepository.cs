namespace SecretMessageSharingWebApp.Repositories.Interfaces
{
    public interface IGeneralRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> Get(string id);

        IQueryable<TEntity> GetAll();

        void Insert(TEntity entity, bool save = false);

        void Delete(TEntity entity, bool save = false);
    }
}
