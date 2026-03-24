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
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

// 1. CONEXIÓN A BASE DE DATOS BLINDADA (Fuerza bruta como respaldo)
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                       ?? "Server=db45210.public.databaseasp.net,1433;Database=db45210;User Id=db45210;Password=j@8SQ4h?5%Ar;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(connectionString));

// Inyeccion de Dependencias
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

// 2. CONFIGURACIÓN CORS (Indispensable para que Fetch API funcione desde el front)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
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
app.Run();