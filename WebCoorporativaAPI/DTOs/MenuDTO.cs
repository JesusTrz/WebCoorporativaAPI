namespace WebCoorporativaAPI.DTOs
{
    public class MenuDTO
    {
        public int ModuloId { get; set; }
        public string Nombre { get; set; }
        public string Ruta { get; set; }
        public string Icono { get; set; }

        public bool Agregar { get; set; }
        public bool Editar { get; set; }
        public bool Eliminar { get; set; }
        public bool Consultar { get; set; }
        public bool Detalle { get; set; }
    }
}
