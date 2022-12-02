using System.ComponentModel.DataAnnotations;

namespace Residence.DTOs;

public class TempResidenceRegisterRequestAdminDto
{
    [Required]
    public int IdHoSoDkiTamtru { get; set; }
    [Required]
    public string? TrangThai { get; set; }
}