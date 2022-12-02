using Residence.Enums;

namespace Residence.Entities;

public class Admin
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public Role Role { get; set; } = Role.Administrator;
}