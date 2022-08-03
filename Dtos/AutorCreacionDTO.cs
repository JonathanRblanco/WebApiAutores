using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Dtos
{
    public class AutorCreacionDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(maximumLength: 120, ErrorMessage = "El nombre no puede tener más de 120 caracteres")]
        public string Nombre { get; set; }
    }
}
