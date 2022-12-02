using System.ComponentModel.DataAnnotations;

namespace Residence.DTOs;

public class TempResidenceExtensionRequestDto
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
    [Required]
    public string? ThuTuc { get; set; }
    public string? TrangThai { get; set; }
    [Required]
    public int IDToKhai { get; set; }
}