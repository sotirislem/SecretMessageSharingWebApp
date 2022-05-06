using SecretsManagerWebApp.Models.DbContext;

namespace SecretsManagerWebApp.Repositories
{
	public interface ISecretMessagesRepository
	{
		SecretMessage Store(SecretMessage secretMessage);

		Task<SecretMessage> Get(string id);
	}
}
