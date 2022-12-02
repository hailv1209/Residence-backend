namespace Residence.Entities;

public class CitizenDeclaration
{
    public int IDToKhai { get; set; }    
    public string? TenGiayTo { get; set; }
    public string? File { get; set; }
    public DateTime UploadedAt { get; set; }
}