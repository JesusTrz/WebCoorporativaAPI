using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;
using WebCoorporativaAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Conexion
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add services to the container.
builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(connectionString));

// Inyeccion de Dependencias
/* EJEMPLO:
 builder.Services.AddScoped<IDeviceService, DeviceService>();
 builder.Services.AddScoped<IDeviceConfigHistoryService, DeviceConfigHistoryService>();
 */

builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
builder.Services.AddScoped<IPerfilService, PerfilService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
