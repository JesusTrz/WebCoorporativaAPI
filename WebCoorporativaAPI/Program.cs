using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;
using WebCoorporativaAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true; // Prueba Header 
// Conexion
// Intentamos leerlo de appsettings, y si no está, lo forzamos a leer directo del entorno de Linux (Railway)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                       ?? builder.Configuration["ConnectionStrings__DefaultConnection"];

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("¡Error Crítico! No se encontró la cadena de conexión en el servidor.");
}

// Add services to the container.
builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(connectionString));

// Inyeccion de Dependencias
/* EJEMPLO:
 builder.Services.AddScoped<IDeviceService, DeviceService>();
 builder.Services.AddScoped<IDeviceConfigHistoryService, DeviceConfigHistoryService>();
 */

builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>)); // Agrega el servicio genérico para todas las entidades
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IModuloService, ModuloService>();
builder.Services.AddScoped<IPermisosPerfilService, PermisosPerfilService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddAuthorization();

var jwtServices = builder.Configuration.GetSection("Jwt");

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
            ValidIssuer = jwtServices["Issuer"],
            ValidAudience = jwtServices["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtServices["Key"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

                // Log de bytes para detectar caracteres invisibles
                if (authHeader != null)
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(authHeader);
                    Console.WriteLine($"=== HEADER BYTES[0-20]: {string.Join(",", bytes.Take(20))}");
                    Console.WriteLine($"=== HEADER COMPLETO LENGTH: {authHeader.Length}");
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"=== AUTH FAILED: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
