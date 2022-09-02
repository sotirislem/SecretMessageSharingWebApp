using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Data.Entities;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Repositories.Interfaces;

namespace SecretMessageSharingWebApp.Repositories
{
	public class SecretMessagesRepository : GeneralRepository<SecretMessageDto>, ISecretMessagesRepository
	{
		public SecretMessagesRepository(SecretMessagesDbContext context) : base(context)
		{ }

		public async Task<int> DeleteOldMessages()
		{
			var comparisonDateTime = DateTime.Now.ToLocalTimeZone().AddHours(-1);
			var deletedMessages = await DeleteRangeBasedOnPredicate(m => m.CreatedDateTime < comparisonDateTime);

			return deletedMessages;
		}
	}
}
