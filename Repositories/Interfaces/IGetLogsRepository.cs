using SecretMessageSharingWebApp.Models.DbContext;

namespace SecretMessageSharingWebApp.Repositories
{
	public interface IGetLogsRepository
	{
		GetLog Add(GetLog getLog);
	}
}
