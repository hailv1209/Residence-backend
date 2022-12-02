using System.ComponentModel.DataAnnotations;

namespace Residence.DTOs;

public class TempResidenceRegisterRequestDto
{
    [Required]
    public int IdHoSoDkiTamtru { get; set; }
    [Required]
    public string? TamTruThanhPho { get; set; }
    [Required]
    public string? TamTruQuan { get; set; }
    [Required]
    public string? TamTruPhuong { get; set; }
    [Required]
    public string? TamTruDiaChi { get; set; }
    [Required]
    public DateTime? TamTruTuNgay { get; set; }
    [Required]
    public DateTime? TamTruDenNgay { get; set; }
}