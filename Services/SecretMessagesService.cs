using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Repositories;
using SecretMessageSharingWebApp.Mappings;

namespace SecretMessageSharingWebApp.Services
{
	public class SecretMessagesService : ISecretMessagesService
	{
		private readonly ILogger<SecretMessagesService> _logger;
		private readonly ISecretMessagesRepository _secretMessagesRepository;

		public SecretMessagesService(ISecretMessagesRepository secretMessagesRepository, ILogger<SecretMessagesService> logger)
		{
			_secretMessagesRepository = secretMessagesRepository;
			_logger = logger;
		}

		public async Task<SecretMessage?> Retrieve(string id)
		{
			var secretMessageDto = await _secretMessagesRepository.Retrieve(id);

			_logger.LogInformation("SecretMessagesService:Retrieve => ID: {secretMessageId}, Exists: {secretMessageExists}.", id, (secretMessageDto is not null));
			
			return secretMessageDto?.ToSecretMessage();
		}

		public SecretMessage Insert(SecretMessage secretMessage)
		{
			var secretMessageDto = secretMessage.ToSecretMessageDto();
			_secretMessagesRepository.Insert(secretMessageDto, true);

			_logger.LogInformation("SecretMessagesService:Insert => ID: {secretMessageId}.", secretMessageDto.Id);

			return secretMessageDto.ToSecretMessage();
		}

		public async Task<bool> Delete(string id)
		{
			var secretMessageDto = await _secretMessagesRepository.Get(id);
			if (secretMessageDto is null)
			{
				return false;
			}

			_secretMessagesRepository.Delete(secretMessageDto, true);

			_logger.LogInformation("SecretMessagesService:Delete => ID: {secretMessageId}.", secretMessageDto.Id);
			return true;
		}

		public IEnumerable<RecentlyStoredSecretMessage> GetRecentlyStoredSecretMessagesInfo(IEnumerable<string> recentlyStoredSecretMessagesList)
		{
			return _secretMessagesRepository.GetAll()
				.Where(secretMessageDto => recentlyStoredSecretMessagesList.Contains(secretMessageDto.Id))
				.Select(secretMessageDto => secretMessageDto.ToRecentlyStoredSecretMessage())
				.AsEnumerable();
		}
	}
}
