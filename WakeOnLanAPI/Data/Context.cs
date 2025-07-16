using DbData.Entities;
using DbData.Entities.Abstracts;
using Dto.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace DbData
{
    public class Context : MutatedContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }

        private readonly IOptions<SystemUserSettings> _sysUser;

        public Context(DbContextOptions<Context> options, IOptions<SystemUserSettings> sysUser) : base(options)
        {
            _sysUser = sysUser;
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>().HasIndex(e => e.Mac).IsUnique();
            modelBuilder.Entity<User>().Property(u => u.Admin).HasDefaultValue(false);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = _sysUser.Value.UserId,
                    LastUpdateTick = DateTime.Now.Ticks,
                    UserName = _sysUser.Value.UserName,
                    Password = _sysUser.Value.Password,
                    Admin = _sysUser.Value.IsAdmin,
                },
                new User
                {
                    Id = 2,
                    UserName = "g@g.c",
                    Password = "000000",
                    LastUpdateTick = DateTime.Now.Ticks,
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
