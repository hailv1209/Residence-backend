using System.Data;
using System.Security.Cryptography;
using System.Text;
using Residence.DTOs;
using Residence.Entities;
using Residence.Enums;
using Residence.Services;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace Residence.Controllers;

public class AdminAccountController : BaseApiController
{
    private readonly IConfiguration _configuration;
    private readonly TokenService _service;
    public AdminAccountController(IConfiguration configuration)
    {
        _configuration = configuration;
        _service = new TokenService(_configuration);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(AdminLoginDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var user = await GetAdmin(connection, request.Username!);
            if (user == null)
            {
                return BadRequest();
            }
            using var hmac = new HMACSHA512(user.PasswordSalt!);

            var computerHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password!));

            for (int i = 0; i < computerHash.Length; i++)
            {
                if (computerHash[i] != user.PasswordHash![i]) return Unauthorized();
            }
            await connection.CloseAsync();
            var userToReturn = new AdminDto
            {
                FullName = user.FullName,
                Username = user.Username,
                Token = _service.GetToken(user),
                Role = Role.Administrator
            };
            return Ok(userToReturn);
        }
        await connection.CloseAsync();
        return BadRequest();
    }

    [HttpPost("register")]
    public async Task<ActionResult<bool>> Register(AdminRegisterDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var user = await GetAdmin(connection, request.Username!);
            if (user != null)
            {
                await connection.CloseAsync();
                return BadRequest("Username already exists");
            }
            using var hmac = new HMACSHA512();

            var computerHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password!));

            var newUser = new Admin
            {
                Username = request.Username,
                FullName = request.FullName,
                PasswordHash = computerHash,
                PasswordSalt = hmac.Key,
            };

            var result = await CreateAdmin(connection, newUser);
            if (result)
            {
                await connection.CloseAsync();
                return Ok(true);
            }
            await connection.CloseAsync();
            return BadRequest();
        }
        await connection.CloseAsync();
        return BadRequest();
    }


    private async Task<Admin?> GetAdmin(MySqlConnection connection, string username)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM admins WHERE Username = @username;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@username", username);
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var admin = new Admin
                        {
                            Id = reader.GetInt32("Id"),
                            Username = reader.GetString("Username"),
                            FullName = reader.GetString("FullName"),
                            PasswordHash = (byte[]) reader["PasswordHash"],
                            PasswordSalt = (byte[]) reader["PasswordSalt"],
                            Role = Role.Administrator,
                        };
                        return admin;
                    }
                    return null;
                }
                return null;
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    private async Task<bool> CreateAdmin(MySqlConnection connection, Admin user)
    {
        var rows_affected = 0;
            using var command = new MySqlCommand();
            command.Connection = connection;

            string queryString = @"INSERT INTO admins (Username, FullName, PasswordHash, PasswordSalt) VALUES (@Username, @FullName, @PasswordHash, @PasswordSalt);
                                select last_insert_id();";

            command.CommandText = queryString;
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@FullName", user.FullName);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@PasswordSalt", user.PasswordSalt);

            try
            {
                rows_affected = await command.ExecuteNonQueryAsync();
                if (rows_affected > 0)
                {
                    
                    return true;
                }
                return false;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
    }
}