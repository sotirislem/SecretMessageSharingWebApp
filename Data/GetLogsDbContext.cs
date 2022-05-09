using Microsoft.EntityFrameworkCore;
using SecretsManagerWebApp.Models.DbContext;

namespace SecretsManagerWebApp.Data
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
