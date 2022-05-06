using SecretsManagerWebApp.Data;
using SecretsManagerWebApp.Models.DbContext;
using System.Diagnostics;

namespace SecretsManagerWebApp.Repositories
{
	public class SecretMessagesRepository : ISecretMessagesRepository
	{
		private readonly SecretMessageDbContext _context;

		public SecretMessagesRepository(SecretMessageDbContext context)
		{
			this._context = context;
		}

		public async Task<SecretMessage> Get(string id)
		{
			var res = await _context.SecretMessages.FindAsync(id);
			if (res is not null && res.DeleteOnRetrieve)
			{
				_context.Remove(res);
				_context.SaveChanges();
			}

			return res;
		}

		public SecretMessage Store(SecretMessage secretMessage)
		{
			_context.SecretMessages.Add(secretMessage);
			var res = _context.SaveChanges();

			if (res != 1)
			{
				throw new Exception("Failed: _context.SecretMessages.Add");
			}
			return secretMessage;
		}
	}
}
