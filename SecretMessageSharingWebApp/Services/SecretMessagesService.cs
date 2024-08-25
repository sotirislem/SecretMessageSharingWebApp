using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Services.Interfaces;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Services;

public sealed class SecretMessagesService(
	ILogger<SecretMessagesService> logger,
	IHttpContextAccessor httpContextAccessor,
	ISecretMessagesRepository secretMessagesRepository) : ISecretMessagesService
{
	public async Task<SecretMessage> Store(SecretMessage secretMessage)
	{
		var secretMessageEntity = secretMessage.ToEntity();

		await secretMessagesRepository.Insert(secretMessageEntity);

		SaveSecretMessageToRecentlyStoredSecretMessagesList(secretMessageEntity.Id);

		logger.LogInformation("SecretMessagesService:Insert => ID: {secretMessageId}", secretMessageEntity.Id);

		return secretMessageEntity.ToDomain();
	}

	public async Task<(bool exists, OtpSettings? otpSettings)> Exists(string id)
	{
		var secretMessageEntity = await secretMessagesRepository.GetById(id);

		var exists = secretMessageEntity is not null;
		var otpSettings = secretMessageEntity?.Otp.ToDomain();

		return (exists, otpSettings);
	}

	public async Task<SecretMessage?> Retrieve(string id)
	{
		var secretMessageEntity = await secretMessagesRepository.GetById(id);

		var secretMessageExists = secretMessageEntity is not null;
		if (secretMessageExists)
		{
			await secretMessagesRepository.Delete(secretMessageEntity!);
		}

		logger.LogInformation("SecretMessagesService:Retrieve => ID: {secretMessageId}, Exists: {secretMessageExists}",
			id, secretMessageExists);

		return secretMessageEntity?.ToDomain();
	}

	public async Task<bool> Delete(string id)
	{
		var secretMessageEntity = await secretMessagesRepository.GetById(id);

		var deleted = false;

		if (secretMessageEntity is not null)
		{
			var deleteResult = await secretMessagesRepository.Delete(secretMessageEntity);
			deleted = deleteResult > 0;
		}

		logger.LogInformation("SecretMessagesService:Delete => ID: {secretMessageId}, Deleted: {deleted}", id, deleted);

		return deleted;
	}

	public async Task<List<RecentlyStoredSecretMessage>> GetRecentlyStoredSecretMessagesInfo(List<string> recentlyStoredSecretMessagesList)
	{
		return (await secretMessagesRepository
			.SelectEntitiesWhere(secretMessageEntity => recentlyStoredSecretMessagesList.Contains(secretMessageEntity.Id)))
			.Select(secretMessageEntity => secretMessageEntity.ToRecentlyStoredSecretMessage())
			.ToList();
	}

	private void SaveSecretMessageToRecentlyStoredSecretMessagesList(string secretMessageId)
	{
		var session = httpContextAccessor.HttpContext!.Session;

		var recentlyStoredSecretMessagesList = session.GetObject<List<string>>(Constants.SessionKeys.RecentlyStoredSecretMessagesList) ?? [];
		recentlyStoredSecretMessagesList.Add(secretMessageId);

		session.SetObject(Constants.SessionKeys.RecentlyStoredSecretMessagesList, recentlyStoredSecretMessagesList);
	}
}
