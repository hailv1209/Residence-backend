using System.ComponentModel.DataAnnotations;

namespace Residence.DTOs;

public class QuestionRequestDto
{
    [Required]
    public string? CauHoi { get; set; }
}