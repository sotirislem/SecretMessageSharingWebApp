using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Data.Dto;

namespace SecretMessageSharingWebApp.Repositories
{
	public class SecretMessagesRepository : GeneralRepository<SecretMessageDto>, ISecretMessagesRepository
	{
		public SecretMessagesRepository(SecretMessagesDbContext context) : base(context)
		{ }

		public async Task<SecretMessageDto?> Retrieve(string id)
		{
			var secretMessageDto = await base.Get(id);
			if (secretMessageDto is not null && secretMessageDto.DeleteOnRetrieve)
			{
				base.Delete(secretMessageDto, true);
			}

			return secretMessageDto;
		}

		public int DeleteOldMessages()
		{
			var oldMessages = _dbSet.Where(m => m.CreatedDateTime < DateTime.Now.AddHours(-1));
			if (oldMessages.Any())
			{
				_dbSet.RemoveRange(oldMessages);
				var dbSaveResult = Save();

				return dbSaveResult;
			}

			return 0;
		}
	}
}
