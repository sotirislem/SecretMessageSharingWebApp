using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Models.Entities;

namespace SecretMessageSharingWebApp.Data
{
	public class SecretMessagesDbContext : DbContext
	{
		public DbSet<SecretMessage> SecretMessages { get; set; }
		public DbSet<GetLog> GetLogs { get; set; }

		public SecretMessagesDbContext(DbContextOptions<SecretMessagesDbContext> options) : base(options)
		{ }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<GetLog>()
				.Property(p => p.Id)
				.ValueGeneratedOnAdd()
				.UseIdentityColumn();

			modelBuilder.Entity<GetLog>()
				.HasIndex(p => p.SecretMessageId);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
		}
	}
}
