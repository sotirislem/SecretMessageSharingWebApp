using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Repositories;

public sealed class SecretMessagesRepository : GeneralRepository<SecretMessageDto>, ISecretMessagesRepository
{
	public SecretMessagesRepository(SecretMessagesDbContext context, IDateTimeProviderService dateTimeProviderService) : base(context, dateTimeProviderService)
	{ }

	public async Task<int> DeleteOldMessages()
	{
		var comparisonDateTime = _dateTimeProviderService.LocalNow().AddHours(-1);
		var deletedMessages = await DeleteRangeBasedOnPredicate(m => m.CreatedDateTime < comparisonDateTime);

		return deletedMessages;
	}
}
