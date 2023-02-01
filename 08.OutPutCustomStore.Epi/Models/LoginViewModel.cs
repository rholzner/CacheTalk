using System.ComponentModel.DataAnnotations;

namespace _08.OutPutCustomStore.Epi.Models;

public class LoginViewModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}
