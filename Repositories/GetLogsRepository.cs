using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Models.Entities;

namespace SecretMessageSharingWebApp.Repositories
{
	public class GetLogsRepository : GeneralRepository<GetLog>, IGetLogsRepository
	{
		public GetLogsRepository(SecretMessagesDbContext context) : base (context)
		{

		}
	}
}
