using Microsoft.AspNetCore.Identity;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(AppDBContext context, UserManager<ApplicationUser> userManager)
        {
            // 1. Asegurarnos de que la base de datos exista
            await context.Database.EnsureCreatedAsync();

            // 2. Sembrar el Perfil primero (porque el usuario depende de este ID)
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

            // 3. Sembrar el Usuario usando Identity para encriptar la contraseña
            if (await userManager.FindByNameAsync("admin") == null)
            {
                // Obtenemos el ID del perfil que acabamos de crear
                var perfil = context.Perfiles.FirstOrDefault(p => p.strNombrePerfil == "Administrador Master");

                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@empresa.com",
                    IdPerfil = perfil.IdPerfil, // Lo vinculamos al perfil
                    Activo = true,              // Requisito: Estado Activo
                    Imagen = null               // Requisito: Imagen nula
                };

                // El UserManager se encarga de crear el usuario y hashear esta contraseña
                var result = await userManager.CreateAsync(adminUser, "Admin123456!");

                if (!result.Succeeded)
                {
                    Console.WriteLine("Error al crear el usuario semilla.");
                }
            }
        }
    }
}
