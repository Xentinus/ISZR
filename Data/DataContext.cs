using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ISZR.Models;

namespace ISZR.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<ISZR.Models.User> User { get; set; } = default!;

        public DbSet<ISZR.Models.Camera> Camera { get; set; }

        public DbSet<ISZR.Models.Class> Class { get; set; }

        public DbSet<ISZR.Models.FonixPermission> FonixPermission { get; set; }

        public DbSet<ISZR.Models.Position> Position { get; set; }

        public DbSet<ISZR.Models.WindowsPermission> WindowsPermission { get; set; }

        public DbSet<ISZR.Models.Group> Group { get; set; }
    }
}