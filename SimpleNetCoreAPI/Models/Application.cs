using SimpleNetCoreAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimpleNetCoreAPI.Models
{
    public class Application
    {
        [Key]
        public int Guid { get; set; }
        public DateTime Date { get; set; }
        public ApplicationType Type { get; set; }
        public ApplicationStatus Status { get; set; }
        public string? Message { get; set; }
    }
}
