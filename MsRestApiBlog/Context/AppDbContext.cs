using Microsoft.EntityFrameworkCore;

namespace MsRestApiBlog.Context
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}
		public DbSet<Models.Blog> Blogs { get; set; }

	}
}
