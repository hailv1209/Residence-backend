namespace Residence.DTOs;

public class QuestionAnswerDto
{
    public int? IdTraLoi { get; set; }
    public int IdCauHoi { get; set; }
    public string? CauHoi { get; set; }
    public DateTime CauHoiUpdatedAt { get; set; }
    public string? CauTraLoi { get; set; }
    public DateTime? CauTraLoiUpdatedAt { get; set; }
}