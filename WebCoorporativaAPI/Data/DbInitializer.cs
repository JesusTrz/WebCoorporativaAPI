using Microsoft.AspNetCore.Identity;
using WebCoorporativaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace WebCoorporativaAPI.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(AppDBContext context, UserManager<ApplicationUser> userManager)
        {
            // 1. Asegurarnos de que la base de datos exista
            await context.Database.EnsureCreatedAsync();

            // --- NUEVA SECCIÓN: SEMBRAR MÓDULOS ---
            await SeedModulosAsync(context);

            // 2. Sembrar el Perfil
            if (!context.Perfiles.Any(p => p.strNombrePerfil == "Administrador Master"))
            {
                var perfilAdmin = new PerfilModel
                {
                    strNombrePerfil = "Administrador Master",
                    BitAdministrador = true
                };

                context.Perfiles.Add(perfilAdmin);
                await context.SaveChangesAsync();
            }

            // 3. Sembrar el Usuario
            if (await userManager.FindByNameAsync("admin") == null)
            {
                var perfil = await context.Perfiles.FirstOrDefaultAsync(p => p.strNombrePerfil == "Administrador Master");

                if (perfil != null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = "admin@empresa.com",
                        IdPerfil = perfil.IdPerfil,
                        Activo = true,
                        Imagen = null
                    };

                    await userManager.CreateAsync(adminUser, "Admin123456!");
                }
            }
        }

        private static async Task SeedModulosAsync(AppDBContext context)
        {
            // Definimos la lista de módulos según tu tabla
            var listaModulos = new List<ModuloModel>
            {
                new ModuloModel { strNombreModulo = "Modulos", Ruta = "/Module/Modulos", Clave = "modulo" },
                new ModuloModel { strNombreModulo = "Perfil", Ruta = "/Module/Perfil", Clave = "perfil" },
                new ModuloModel { strNombreModulo = "PermisosPerfil", Ruta = "/Module/PermisosPerfil", Clave = "permisosperfil" },
                new ModuloModel { strNombreModulo = "Usuario", Ruta = "/Module/Usuario", Clave = "usuario" },
                new ModuloModel { strNombreModulo = "Principal 1.1", Ruta = "/Principales/PrincipalU1", Clave = "principal11" },
                new ModuloModel { strNombreModulo = "Principal 1.2", Ruta = "/Principales/PrincipalU2", Clave = "principal12" },
                new ModuloModel { strNombreModulo = "Principal 2.1", Ruta = "/Principales/PrincipalD1", Clave = "principal21" },
                new ModuloModel { strNombreModulo = "Principal 2.2", Ruta = "/Principales/PrincipalD2", Clave = "principal22" }
            };

            foreach (var mod in listaModulos)
            {
                // Verificamos si la Clave ya existe para no duplicar datos en cada ejecución
                if (!await context.Modulos.AnyAsync(m => m.Clave == mod.Clave))
                {
                    context.Modulos.Add(mod);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}