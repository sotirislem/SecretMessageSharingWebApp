using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Data.Dto;

namespace SecretMessageSharingWebApp.Repositories
{
	public class GetLogsRepository : GeneralRepository<GetLogDto>, IGetLogsRepository
	{
		public GetLogsRepository(SecretMessagesDbContext context) : base(context)
		{ }
	}
}
