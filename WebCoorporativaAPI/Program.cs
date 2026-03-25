using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.Helpers;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;
using WebCoorporativaAPI.Services;

var builder = WebApplication.CreateBuilder(args);
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

// 1. CONEXIÓN A BASE DE DATOS BLINDADA (Fuerza bruta como respaldo)
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                       ?? "Server=db45210.public.databaseasp.net,1433;Database=db45210;User Id=db45210;Password=j@8SQ4h?5%Ar;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(connectionString));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Inyeccion de Dependencias
builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IModuloService, ModuloService>();
builder.Services.AddScoped<IPermisosPerfilService, PermisosPerfilService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddAuthorization();

// 2. CONFIGURACIÓN CORS (Indispensable para que Fetch API funcione desde el front)
// 2. CONFIGURACIÓN CORS (Indispensable para que Fetch API funcione desde el front)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5017",   // Puerto HTTP por defecto
                "https://localhost:7079",  // Puerto HTTPS por defecto
                "http://localhost:5017",   // (Añade aquí el puerto exacto de tu Razor Pages)
                "https://localhost:7079"   // (Añade aquí el puerto exacto de tu Razor Pages)
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Vital si en algún momento envías cookies o tokens específicos
    });
});

// 3. CONFIGURACIÓN JWT BLINDADA
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "MindCorp@WebCoorporativa#2026$SecretKey!JWT@Secure123456789";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "WebCorporativaAPI";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "WebCorporativaAPI";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

// 4. PIPELINE DE PRODUCCIÓN
app.UseCors("PermitirFrontend");

// Quitamos el 'if' de Development para que el profe pueda evaluar Swagger en la URL pública
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 5. BLOQUE DE SEEDING (Semilla de base de datos)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDBContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Ejecutamos nuestra clase semilla
        await DbInitializer.SeedDataAsync(context, userManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error ejecutando el Seed: {ex.Message}");
    }
}

app.Run();

// 6. CLASE INICIALIZADORA (Al final del archivo para respetar Top-Level Statements)
public static class DbInitializer
{
    public static async Task SeedDataAsync(AppDBContext context, UserManager<ApplicationUser> userManager)
    {
        // 1. Asegurarnos de que la base de datos y las tablas existan
        await context.Database.EnsureCreatedAsync();

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
            var perfil = context.Perfiles.FirstOrDefault(p => p.strNombrePerfil == "Administrador Master");

            var adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@empresa.com",
                IdPerfil = perfil.IdPerfil,
                Activo = true,
                Imagen = null
            };

            // Creamos el usuario con contraseña segura
            var result = await userManager.CreateAsync(adminUser, "Admin123456!");

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error Seed Identity: {error.Description}");
                }
            }
        }
    }
}