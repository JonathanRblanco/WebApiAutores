using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Dtos
{
    public class LibroCreacionDTO
    {
        [StringLength(maximumLength: 250)]
        [Required]
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public List<int> AutoresIds { get; set; }
    }
}
