using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Data.Entities;

namespace SecretMessageSharingWebApp.Data
{
	public class SecretMessagesDbContext : DbContext
	{
		public DbSet<SecretMessageDto> SecretMessages { get; set; }
		public DbSet<GetLogDto> GetLogs { get; set; }

		public SecretMessagesDbContext(DbContextOptions<SecretMessagesDbContext> options) : base(options)
		{ }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<GetLogDto>()
				.Property(p => p.Id)
				.ValueGeneratedOnAdd()
				.UseIdentityColumn();

			modelBuilder.Entity<GetLogDto>()
				.HasIndex(p => p.SecretMessageId);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
		}
	}
}
