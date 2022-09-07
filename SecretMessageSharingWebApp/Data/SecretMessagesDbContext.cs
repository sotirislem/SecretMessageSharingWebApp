using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Data.Entities;

namespace SecretMessageSharingWebApp.Data
{
	public class SecretMessagesDbContext : DbContext
	{
		public SecretMessagesDbContext(DbContextOptions<SecretMessagesDbContext> options) : base(options)
		{ }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<SecretMessageDto>()
				.ToContainer("SecretMessages")
				.HasNoDiscriminator();

			modelBuilder.Entity<GetLogDto>()
				.ToContainer("GetLogs")
				.HasNoDiscriminator();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
		}

		public DbSet<SecretMessageDto> SecretMessages { get; set; }
		public DbSet<GetLogDto> GetLogs { get; set; }
	}
}
