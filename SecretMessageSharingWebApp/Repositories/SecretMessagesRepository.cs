using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Repositories.Interfaces;

namespace SecretMessageSharingWebApp.Repositories
{
	public class SecretMessagesRepository : GeneralRepository<SecretMessageDto>, ISecretMessagesRepository
	{
		public SecretMessagesRepository(SecretMessagesDbContext context) : base(context)
		{ }

		public int DeleteOldMessages()
		{
			var comparisonDateTime = DateTime.Now.AddHours(-1);

			var oldMessages = _dbSet.Where(m => m.CreatedDateTime < comparisonDateTime);
			if (oldMessages.Count() > 0)
			{
				_dbSet.RemoveRange(oldMessages);
				var dbSaveResult = Save();

				return dbSaveResult;
			}

			return 0;
		}
	}
}
