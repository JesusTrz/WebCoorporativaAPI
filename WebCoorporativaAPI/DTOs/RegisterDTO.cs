namespace WebCoorporativaAPI.DTOs
{
    public class RegisterDTO
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int IdPerfil { get; set; }
        public bool Activo { get; set; }
    }
}
