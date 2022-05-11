using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Models.DbContext;

namespace SecretMessageSharingWebApp.Data
{
	public class GetLogsDbContext : DbContext
	{
		public DbSet<GetLog> GetLogs { get; set; }

		public GetLogsDbContext(DbContextOptions<GetLogsDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<GetLog>()
				.Property(p => p.Id)
				.ValueGeneratedOnAdd()
				.UseIdentityColumn();
		}
	}
}
