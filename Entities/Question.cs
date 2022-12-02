namespace Residence.Entities;

public class Question
{
    public int IdCauHoi { get; set; }    
    public string? CauHoi { get; set; }
    public int IdUsers { get; set; }
    public int IdTraLoi { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}