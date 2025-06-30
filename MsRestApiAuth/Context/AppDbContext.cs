using Microsoft.EntityFrameworkCore;
using MsRestApiAuth.Domain.Entities;

namespace MsRestApiAuth.Context
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Notification> Notifications { get; set; }
	}
}
