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

public class AccountController : BaseApiController
{
    private readonly IConfiguration _configuration;
    private readonly TokenService _service;
    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
        _service = new TokenService(_configuration);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var user = await GetUser(connection, request.Username!);
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
            var userToReturn = new UserDto
            {
                Fullname = user.Fullname,
                Username = user.Username,
                Token = _service.GetToken(user)
            };
            return Ok(userToReturn);
        }
        await connection.CloseAsync();
        return BadRequest();
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var user = await GetUser(connection, request.Username!);
            if (user != null)
            {
                await connection.CloseAsync();
                return BadRequest("Username already exists");
            }
            using var hmac = new HMACSHA512();

            var computerHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password!));

            var newUser = new User
            {
                Username = request.Username,
                Fullname = request.Fullname,
                Gender = request.Gender,
                Birthday = request.Birthday,
                City = request.City,
                District = request.District,
                Ward = request.Ward,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                PasswordHash = computerHash,
                PasswordSalt = hmac.Key,
            };

            var result = await CreateUser(connection, newUser);
            if (result)
            {
                var createdUser = await GetUser(connection, request.Username!);
                if (createdUser == null)
                {
                    return BadRequest();
                }
                await connection.CloseAsync();
                var userToReturn = new UserDto
                {
                    Fullname = createdUser.Fullname,
                    Username = createdUser.Username,
                    Token = _service.GetToken(createdUser)
                };
                return Ok(userToReturn);
            }
            await connection.CloseAsync();
            return BadRequest();
        }
        await connection.CloseAsync();
        return BadRequest();
    }


    private async Task<User?> GetUser(MySqlConnection connection, string username)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM users WHERE Username = @username;";

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
                        var user = new User
                        {
                            IdUsers = reader.GetInt32("IdUsers"),
                            Username = reader.GetString("Username"),
                            Fullname = reader.GetString("Fullname"),
                            PasswordHash = (byte[])reader["PasswordHash"],
                            PasswordSalt = (byte[])reader["PasswordSalt"],
                        };
                        return user;
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

    private async Task<bool> CreateUser(MySqlConnection connection, User user)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"INSERT INTO users (Username, Fullname, Gender, Birthday, City, District, Ward, Email, Phone, Address, PasswordHash, PasswordSalt) VALUES (@Username, @Fullname, @Gender, @Birthday, @City, @District, @Ward, @Email, @Phone, @Address, @PasswordHash, @PasswordSalt);
                                select last_insert_id();";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@Fullname", user.Fullname);
        command.Parameters.AddWithValue("@Gender", user.Gender);
        command.Parameters.AddWithValue("@Birthday", user.Birthday);
        command.Parameters.AddWithValue("@City", user.City);
        command.Parameters.AddWithValue("@District", user.District);
        command.Parameters.AddWithValue("@Ward", user.Ward);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@Phone", user.Phone);
        command.Parameters.AddWithValue("@Address", user.Address);
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