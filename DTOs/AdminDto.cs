using Residence.Enums;

namespace Residence.DTOs;

public class AdminDto
{
    public string? FullName { get; set; }    
    public string? Username { get; set; }
    public string? Token { get; set; }

    public Role Role { get; set; } = Role.Administrator;
}