using System.ComponentModel.DataAnnotations;
using ims_inv.Events;

namespace ims_inv.Models
{
    public class User : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }


    public class CreateUserViewModel
    {
        public string? Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }


    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
