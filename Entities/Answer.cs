namespace Residence.Entities;

public class Answer
{
    public int IdTraLoi { get; set; }    
    public int IdCauHoi { get; set; }
    public string? CauTraLoi { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}