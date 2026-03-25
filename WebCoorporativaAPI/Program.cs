using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;
using WebCoorporativaAPI.Services;

AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    var ex = e.ExceptionObject as Exception;
    File.AppendAllText("C:/crash_api.txt",
        $"[{DateTime.Now}] {ex?.Message}\n{ex?.StackTrace}\n{ex?.InnerException?.Message}\n\n");
};

var builder = WebApplication.CreateBuilder(args);
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

// 🔗 CONEXIÓN A DB
// 1. CONEXIÓN A BASE DE DATOS BLINDADA (Fuerza bruta como respaldo)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


//var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
//                       ?? "Server=db45210.public.databaseasp.net,1433;Database=db45210;User Id=db45210;Password=j@8SQ4h?5%Ar;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(connectionString));

// 🔌 DEPENDENCIAS
builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IModuloService, ModuloService>();
builder.Services.AddScoped<IPermisosPerfilService, PermisosPerfilService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddAuthorization();

// 🌐 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5017",
                "https://localhost:7079"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 🔐 JWT
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
             ?? "MindCorp@WebCoorporativa#2026$SecretKey!JWT@Secure123456789";

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "WebCorporativaAPI";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "WebCorporativaAPI";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

var app = builder.Build();

// 🚀 PIPELINE
app.UseCors("PermitirFrontend");

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// 🔥 IMPORTANTE PARA VER IMÁGENES
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 🌱 SEED
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDBContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await DbInitializer.SeedDataAsync(context, userManager);
        Console.WriteLine("Seed ejecutado correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error ejecutando el Seed: {ex.Message}");
        if (ex.InnerException != null)
            Console.WriteLine($"Inner: {ex.InnerException.Message}");
    }
}

app.Run();

// 🌱 DB INITIALIZER
public static class DbInitializer
{
    public static async Task SeedDataAsync(AppDBContext context, UserManager<ApplicationUser> userManager)
    {
        await context.Database.EnsureCreatedAsync();

        await SeedModulosAsync(context);

        if (!await context.Perfiles.AnyAsync(p => p.strNombrePerfil == "Administrador Master"))
        {
            var perfilAdmin = new PerfilModel
            {
                strNombrePerfil = "Administrador Master",
                BitAdministrador = true
            };
            context.Perfiles.Add(perfilAdmin);
            await context.SaveChangesAsync();
        }

        if (await userManager.FindByNameAsync("admin") == null)
        {
            var perfil = await context.Perfiles
                .FirstOrDefaultAsync(p => p.strNombrePerfil == "Administrador Master");

            if (perfil != null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@empresa.com",
                    IdPerfil = perfil.IdPerfil,
                    Activo = true
                };

                await userManager.CreateAsync(adminUser, "Admin123456!");
            }
        }
    }

    private static async Task SeedModulosAsync(AppDBContext context)
    {
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
            var existe = await context.Modulos
                .AnyAsync(m => m.Clave.ToLower() == mod.Clave.ToLower());

            if (!existe)
                context.Modulos.Add(mod);
        }

        await context.SaveChangesAsync();
    }
}