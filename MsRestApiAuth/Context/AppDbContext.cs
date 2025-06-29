using Microsoft.EntityFrameworkCore;
using MsRestApiAuth.Models;

namespace MsRestApiAuth.Context
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		public DbSet<User> Users { get; set; }
	}
}
