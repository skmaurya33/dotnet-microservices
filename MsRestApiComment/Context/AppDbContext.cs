using Microsoft.EntityFrameworkCore;
using MsRestApiComment.Models;

namespace MsRestApiComment.Context
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}
		public DbSet<Comment> Comments { get; set; }

	}

}
