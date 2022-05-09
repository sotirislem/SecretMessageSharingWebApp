using SecretsManagerWebApp.Models.DbContext;

namespace SecretsManagerWebApp.Repositories
{
	public interface IGetLogsRepository
	{
		GetLog Add(GetLog getLog);
	}
}
