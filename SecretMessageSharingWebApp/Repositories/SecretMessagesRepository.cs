using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Providers;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Repositories;

public sealed class SecretMessagesRepository : GeneralRepository<SecretMessageEntity>, ISecretMessagesRepository
{
	public SecretMessagesRepository(
		SecretMessagesDbContext context,
		IDateTimeProviderService dateTimeProviderService,
		ICancellationTokenProvider cancellationTokenProvider)
		: base(context, dateTimeProviderService, cancellationTokenProvider)
	{ }

	public async Task<int> DeleteOldMessages(CancellationToken ct)
	{
		var comparisonDateTime = _dateTimeProviderService
			.LocalNow()
			.AddHours(Constants.DeleteOldMessagesAfterHours * -1);

		var deletedMessages = await DeleteRangeBasedOnPredicate(m => m.CreatedDateTime < comparisonDateTime, ct);
		return deletedMessages;
	}
}
