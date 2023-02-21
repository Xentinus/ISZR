using ISZR.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ISZR.Web.Data
{
	public class DataContext : DbContext
	{
		public DataContext(DbContextOptions<DataContext> options)
			: base(options)
		{
		}

		public DbSet<User> Users { get; set; } = default!;

		public DbSet<Camera> Cameras { get; set; } = default!;

		public DbSet<Class> Classes { get; set; } = default!;

		public DbSet<Position> Positions { get; set; } = default!;

		public DbSet<Request> Requests { get; set; } = default!;

		public DbSet<Permission> Permissions { get; set; } = default!;

		public DbSet<Group> Groups { get; set; } = default!;
	}
}