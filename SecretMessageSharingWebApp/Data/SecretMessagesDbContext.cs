using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Data.Entities;

namespace SecretMessageSharingWebApp.Data;

public sealed class SecretMessagesDbContext : DbContext
{
	public SecretMessagesDbContext(DbContextOptions<SecretMessagesDbContext> options) : base(options)
	{ }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<SecretMessageEntity>()
			.HasKey(e => e.Id);

		modelBuilder.Entity<GetLogEntity>()
			.HasKey(e => e.Id);

		modelBuilder.Entity<SecretMessageEntity>()
			.ToContainer("SecretMessages")
			.HasNoDiscriminator();

		modelBuilder.Entity<GetLogEntity>()
			.ToContainer("GetLogs")
			.HasNoDiscriminator();
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var logLevel = LogLevel.Warning;
#if DEBUG
		logLevel = LogLevel.Information;
#endif
		optionsBuilder.LogTo(Console.WriteLine, logLevel);
	}

	public DbSet<SecretMessageEntity> SecretMessages { get; init; }
	public DbSet<GetLogEntity> GetLogs { get; init; }
}
