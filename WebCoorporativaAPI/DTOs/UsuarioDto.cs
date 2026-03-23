namespace WebCoorporativaAPI.DTOs
{
    public class UsuarioDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public int IdPerfil { get; set; }
        public string NombrePerfil { get; set; }
        public bool Activo { get; set; }
        public string? Imagen { get; set; }
    }
}
