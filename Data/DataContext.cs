using ISZR.Models;
using Microsoft.EntityFrameworkCore;

namespace ISZR.Data
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

        public DbSet<FonixPermission> FonixPermissions { get; set; } = default!;

        public DbSet<Position> Positions { get; set; } = default!;

        public DbSet<WindowsPermission> WindowsPermissions { get; set; } = default!;

        public DbSet<ISZR.Models.Request> Request { get; set; }
    }
}