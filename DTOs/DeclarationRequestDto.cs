using System.ComponentModel.DataAnnotations;

namespace Residence.DTOs;

public class DeclarationRequestDto
{
    [Required]
    public string? TenGiayTo { get; set; }
    [Required]
    public IFormFile? File { get; set; }
}