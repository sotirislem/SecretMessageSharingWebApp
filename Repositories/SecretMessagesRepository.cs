using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Models.Entities;

namespace SecretMessageSharingWebApp.Repositories
{
	public class SecretMessagesRepository : GeneralRepository<SecretMessage>, ISecretMessagesRepository
	{
		private readonly ILogger<SecretMessagesRepository> _logger;

		public SecretMessagesRepository(SecretMessagesDbContext context, ILogger<SecretMessagesRepository> logger) : base(context)
		{
			_logger = logger;
		}

		public override void Insert(SecretMessage secretMessage, bool save = false)
		{
			base.Insert(secretMessage, save);
			_logger.LogInformation("SecretMessages:Insert => ID: {secretMessageId}.", secretMessage.Id);
		}

		public SecretMessage? Retrieve(string id)
		{
			var secretMessage = base.Get(id);
			if (secretMessage is not null && secretMessage.DeleteOnRetrieve)
			{
				base.Delete(secretMessage, true);
			}

			_logger.LogInformation("SecretMessages:Retrieve => ID: {secretMessageId}, Exists: {secretMessageExists}.", id, (secretMessage is not null));
			return secretMessage;
		}

		public void DeleteOldMessages()
		{
			var oldMessages = _dbSet.Where(m => m.CreatedDateTime < DateTime.Now.AddHours(-1));
			if (oldMessages.Any())
			{
				_dbSet.RemoveRange(oldMessages);
				var dbSaveResult = Save();

				_logger.LogInformation("SecretMessagesRepository: Deleted {dbSaveResult} old message(s).", dbSaveResult);
			}
		}
	}
}
