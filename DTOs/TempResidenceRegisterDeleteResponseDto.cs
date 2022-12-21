namespace Residence.DTOs;

public class TempResidenceRegisterDeleteResponseDto
{
    public int IdHoSoXoaGiaHan  { get; set; }
    public int IdHoSoDkiTamtru { get; set; }
    public int IdUsers { get; set; }
    public string? ThuTuc { get; set; }
    public string? ThanhPho { get; set; }
    public string? Quan { get; set; }
    public string? Phuong { get; set; }
    public string? DiaChi { get; set; }
    public string? HoTenChuHo { get; set; }
    public string? QuanHeVoiChuHo { get; set; }
    public string? CMNDChuHo { get; set; }
    public string? NoiDung { get; set; }
    public DateTime? TamTruTuNgay { get; set; }
    public DateTime? TamTruDenNgay { get; set; }
    public int IdToKhai { get; set; }
    public string? TrangThai { get; set; }
}