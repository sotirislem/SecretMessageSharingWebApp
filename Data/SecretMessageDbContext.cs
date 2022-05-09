using Microsoft.EntityFrameworkCore;
using SecretsManagerWebApp.Models.DbContext;

namespace SecretsManagerWebApp.Data
{
	public class SecretMessageDbContext : DbContext
	{
		public DbSet<SecretMessage> SecretMessages { get; set; }

		public SecretMessageDbContext(DbContextOptions<SecretMessageDbContext> options) : base(options) { }
	}
}
