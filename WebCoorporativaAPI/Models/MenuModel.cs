using System.ComponentModel.DataAnnotations;

namespace WebCoorporativaAPI.Models
{
    public class MenuModel
    {
        [Key]
        public int IdMmodel { get; set; }

        public int IdMenu { get; set; }

        public int IdModulo { get; set; }
        public Models.ModuloModel Modulo { get; set; }

    }
}
