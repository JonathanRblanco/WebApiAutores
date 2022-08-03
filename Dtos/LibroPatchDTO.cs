using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Dtos
{
    public class LibroPatchDTO
    {
        [StringLength(maximumLength: 250)]
        [Required]
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}
