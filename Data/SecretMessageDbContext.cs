using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Models.DbContext;

namespace SecretMessageSharingWebApp.Data
{
	public class SecretMessageDbContext : DbContext
	{
		public DbSet<SecretMessage> SecretMessages { get; set; }

		public SecretMessageDbContext(DbContextOptions<SecretMessageDbContext> options) : base(options) { }
	}
}
