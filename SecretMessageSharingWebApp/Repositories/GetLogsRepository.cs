using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Data.Dto;
using SecretMessageSharingWebApp.Repositories.Interfaces;

namespace SecretMessageSharingWebApp.Repositories
{
	public class GetLogsRepository : GeneralRepository<GetLogDto>, IGetLogsRepository
	{
		public GetLogsRepository(SecretMessagesDbContext context) : base(context)
		{ }
	}
}
