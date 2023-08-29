using ISZR.Web.Models;

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

        public DbSet<Phone> Phones { get; set; } = default!;

        public DbSet<Parking> Parkings { get; set; } = default!;

        public DbSet<Report> Reports { get; set; } = default!;
        public DbSet<GroupPermission> GroupPermissions { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupPermission>()
                .HasKey(gp => new { gp.GroupId, gp.PermissionId });

            modelBuilder.Entity<GroupPermission>()
                .HasOne(gp => gp.Group)
                .WithMany(g => g.GroupPermissions)
                .HasForeignKey(gp => gp.GroupId);

            modelBuilder.Entity<GroupPermission>()
                .HasOne(gp => gp.Permission)
                .WithMany(p => p.GroupPermissions)
                .HasForeignKey(gp => gp.PermissionId);

            base.OnModelCreating(modelBuilder);
        }
    }
}