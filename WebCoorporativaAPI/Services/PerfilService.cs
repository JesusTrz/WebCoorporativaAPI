using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Services
{
    public class PerfilService : BaseService<PerfilModel>, IPerfilService
    {
        public PerfilService(AppDBContext context) : base(context)
        {
        }
    }
}
