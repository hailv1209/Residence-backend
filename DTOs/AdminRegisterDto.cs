using System.ComponentModel.DataAnnotations;
using Residence.Enums;

namespace Residence.DTOs;

public class AdminRegisterDto
{
    [Required]
    public string? Username { get; set; }    
    [Required]
    public string? FullName { get; set; }
    [Required]
    public string? Password { get; set; }
}