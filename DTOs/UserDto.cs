using Residence.Enums;

namespace Residence.DTOs;

public class UserDto
{
    public string? Fullname { get; set; }    
    public string? Username { get; set; }
    public string? Token { get; set; }

    public Role Role { get; set; }
}