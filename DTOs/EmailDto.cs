namespace Residence.DTOs;

public class EmailDto<T>
{
    public string? Template { get; set; }
    public T? Model { get; set; }
    public string? To { get; set; }
    public string? Subject { get; set; }
}