using SecretsManagerWebApp.Data;
using SecretsManagerWebApp.Models.DbContext;
using System.Diagnostics;

namespace SecretsManagerWebApp.Repositories
{
	public class SecretMessagesRepository : ISecretMessagesRepository
	{
		private readonly SecretMessageDbContext _context;
		private readonly ILogger<SecretMessagesRepository> _logger;

		public SecretMessagesRepository(SecretMessageDbContext context, ILogger<SecretMessagesRepository> logger)
		{
			this._context = context;
			_logger = logger;
		}

		public async Task<SecretMessage> Get(string id)
		{
			var secretMessage = await _context.SecretMessages.FindAsync(id);
			if (secretMessage is not null && secretMessage.DeleteOnRetrieve)
			{
				_context.Remove(secretMessage);
				_context.SaveChanges();
			}

			_logger.LogInformation("SecretMessages:Get => SecretMessage with ID: {secretMessageId}, Exists: {secretMessageExists}.", id, (secretMessage is not null));
			return secretMessage!;
		}

		public SecretMessage Store(SecretMessage secretMessage)
		{
			_context.SecretMessages.Add(secretMessage);
			var dbSaveResult = _context.SaveChanges();
			if (dbSaveResult < 1)
			{
				throw new Exception("SecretMessagesRepository: Store");
			}

			_logger.LogInformation("SecretMessages:Store => new SecretMessage with ID: {secretMessageId}.", secretMessage.Id);
			return secretMessage;
		}

		public void DeleteOldMessages()
		{
			var oldMessages = _context.SecretMessages.Where(m => m.CreatedDateTime < DateTime.Now.AddHours(-1));
			if (oldMessages.Any())
			{
				_context.SecretMessages.RemoveRange(oldMessages);
				var dbSaveResult = _context.SaveChanges();
				if (dbSaveResult < 1)
				{
					throw new Exception("SecretMessagesRepository: DeleteOldMessages");
				}

				_logger.LogInformation("SecretMessagesRepository: Deleted {dbSaveResult} old message(s).", dbSaveResult);
			}
		}
	}
}
