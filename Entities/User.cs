using Residence.Enums;

namespace Residence.Entities;

public class User
{
    public int IdUsers { get; set; }    
    public string? Username { get; set; }
    public string? Fullname { get; set; }
    public string? Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public Role Role { get; set; } = Role.User;
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
}