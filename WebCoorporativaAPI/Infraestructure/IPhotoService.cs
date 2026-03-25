using CloudinaryDotNet.Actions;

namespace WebCoorporativaAPI.Infraestructure
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    }
}
