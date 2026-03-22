using WebCoorporativaAPI.DTOs;

namespace WebCoorporativaAPI.Infraestructure
{
    public interface IMenuService
    {
        Task<List<MenuDTO>> GetMenuByUser(string userId);
    }
}
