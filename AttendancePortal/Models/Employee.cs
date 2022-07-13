using System.ComponentModel.DataAnnotations;

namespace AttendancePortal.Models
{
    public class Employee
    {
        [Required]
        public string UserName { get; set; }
    }
}