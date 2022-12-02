using System.ComponentModel.DataAnnotations;

namespace Residence.DTOs;

public class AnswerRequestDto
{
    [Required]
    public int IdCauHoi { get; set; }
    [Required]
    public string? CauTraLoi { get; set; }
}