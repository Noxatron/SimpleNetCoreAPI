using System.ComponentModel.DataAnnotations;

namespace SimpleNetCoreAPI.Server.Models
{
    public class User
    {
        [Key]
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}
