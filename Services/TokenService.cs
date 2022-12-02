using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Residence.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Residence.Services;

public class TokenService
{
     private readonly SymmetricSecurityKey _key;
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config)
    {
        _config = config;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
    }

    public string GetToken(User user)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.GivenName, user.Fullname!),
            new Claim(ClaimTypes.NameIdentifier, user.Username!)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GetToken(Admin user)
    {
        var claims = new List<Claim> {
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.GivenName, user.FullName!),
            new Claim(ClaimTypes.NameIdentifier, user.Username!)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}