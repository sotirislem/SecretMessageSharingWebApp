﻿using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Services.Interfaces;
using SecretMessageSharingWebApp.Repositories.Interfaces;

namespace SecretMessageSharingWebApp.Services;

public sealed class SecretMessagesService : ISecretMessagesService
{
	private readonly ILogger<SecretMessagesService> _logger;
	private readonly ISecretMessagesRepository _secretMessagesRepository;

	public SecretMessagesService(ISecretMessagesRepository secretMessagesRepository, ILogger<SecretMessagesService> logger)
	{
		_secretMessagesRepository = secretMessagesRepository;
		_logger = logger;
	}

	public async Task<SecretMessage> Store(SecretMessage secretMessage)
	{
		var secretMessageDto = secretMessage.ToSecretMessageDto();
		await _secretMessagesRepository.Insert(secretMessageDto);

		_logger.LogInformation("SecretMessagesService:Insert => ID: {secretMessageId}", secretMessageDto.Id);

		return secretMessageDto.ToSecretMessage();
	}

	public (bool exists, OtpSettings? otp) VerifyExistence(string id)
	{
		var result = _secretMessagesRepository.GetDbSetAsQueryable()
			.Where(m => m.Id == id)
			.Select(m => new { Otp = m.Otp.ToOtpSettings() })
			.FirstOrDefault();

		var exists = (result is not null);

		_logger.LogInformation("SecretMessagesService:VerifyExistance => ID: {secretMessageId}, Exists: {secretMessageExists}, RequiresOtp: {secretMessageRequiresOtp}", id, exists, result?.Otp.Required);

		return (exists, result?.Otp);
	}

	public async Task<SecretMessage?> Retrieve(string id)
	{
		var secretMessageDto = await _secretMessagesRepository.Get(id);
		if (secretMessageDto is not null && secretMessageDto.DeleteOnRetrieve)
		{
			await _secretMessagesRepository.Delete(secretMessageDto);
		}

		_logger.LogInformation("SecretMessagesService:Retrieve => ID: {secretMessageId}, Exists: {secretMessageExists}", id, (secretMessageDto is not null));

		return secretMessageDto?.ToSecretMessage();
	}

	public async Task<bool> Delete(string id)
	{
		var secretMessageDto = await _secretMessagesRepository.Get(id);
		if (secretMessageDto is null)
		{
			return false;
		}

		await _secretMessagesRepository.Delete(secretMessageDto);

		_logger.LogInformation("SecretMessagesService:Delete => ID: {secretMessageId}", secretMessageDto.Id);

		return true;
	}

	public IEnumerable<RecentlyStoredSecretMessage> GetRecentlyStoredSecretMessagesInfo(List<string> recentlyStoredSecretMessagesList)
	{
		return _secretMessagesRepository.GetDbSetAsQueryable()
			.Where(secretMessageDto => recentlyStoredSecretMessagesList.Contains(secretMessageDto.Id))
			.Select(secretMessageDto => secretMessageDto.ToRecentlyStoredSecretMessage())
			.AsEnumerable();
	}
}
