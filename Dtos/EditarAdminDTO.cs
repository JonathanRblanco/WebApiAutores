using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Dtos
{
    public class EditarAdminDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
