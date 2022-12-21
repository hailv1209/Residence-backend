using System.ComponentModel.DataAnnotations;

namespace Residence.DTOs;

public class ResidenceInformationRequestDto
{
    [Required]
    public string? Fullname { get; set; }
    [Required]
    public string? Gender { get; set; }
    [Required]
    public DateTime? Birthday { get; set; }
    [Required]
    public string? City { get; set; }
    [Required]
    public string? District { get; set; }
    [Required]
    public string? Ward { get; set; }
    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Phone { get; set; }
    [Required]
    public string? Address { get; set; }
    // [Required]
    // public int IDToKhai { get; set; }
    // public string? TrangThai { get; set; }
}