using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Data
{
    public class AppDBContext : IdentityDbContext<ApplicationUser>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        // Modelos Ejemplo
        /* public DbSet<Device> Devices { get; set; } */
        public DbSet<PerfilModel> Perfiles { get; set; }
        public DbSet<ModuloModel> Modulos { get; set; }
        public DbSet<PermisosPerfilModel> PermisosPerfil { get; set; }
        public DbSet<MenuModel> Menus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configuraciones adicionales de modelos
            /* modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                // Otras configuraciones...
            }); */
        }
    }
}
