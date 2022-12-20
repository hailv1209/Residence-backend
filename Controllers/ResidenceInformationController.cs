using System.Data;
using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Residence.DTOs;
using Residence.Entities;

namespace Residence.Controllers;

[Authorize(Roles = "User")]

public class ResidenceInformationController : BaseApiController
{
    private readonly ILogger<ResidenceInformationController> _logger;
    private readonly IConfiguration _configuration;

    public ResidenceInformationController(ILogger<ResidenceInformationController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("current")]
    public async Task<ActionResult<UserResponseDto>> GetCurrentUser()
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userFromDB = await GetUser(connection, username);

            if (userFromDB == null)
            {
                await connection.CloseAsync();
                return NotFound("User not found");
            }

            var result = new UserResponseDto
            {
                Gender = userFromDB.Gender,
                Fullname = userFromDB.Fullname,
                Birthday = userFromDB.Birthday,
                City = userFromDB.City,
                District = userFromDB.District,
                Ward = userFromDB.Ward,
                Email = userFromDB.Email,
                Phone = userFromDB.Phone,
                Address = userFromDB.Address,
                Username = username
            };
            return result;

        }
        await connection.CloseAsync();
        return BadRequest();
    }

    [HttpPatch]
    public async Task<ActionResult<bool>> Patch(ResidenceInformationRequestDto residenceInformation)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userFromDB = await GetUser(connection, username);

            if (userFromDB == null)
            {
                await connection.CloseAsync();
                return NotFound("User not found");
            }

            var userUpdateResult = await UpdateUser(connection, residenceInformation, username);

            if (!userUpdateResult)
            {
                await connection.CloseAsync();
                return BadRequest();
            }

            // var residenceInformationResult = await CreateResidenceInformation(connection, userFromDB.IdUsers, residenceInformation);

            // if (!residenceInformationResult)
            // {
            //     await connection.CloseAsync();
            //     return BadRequest();
            // }

            await connection.CloseAsync();
            return Ok(true);

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
                            Gender = reader.GetString("Gender"),
                            Fullname = reader.GetString("Fullname"),
                            Birthday = (DateTime)reader["Birthday"],
                            City = reader.GetString("City"),
                            District = reader.GetString("District"),
                            Ward = reader.GetString("Ward"),
                            Email = reader.GetString("Email"),
                            Phone = reader.GetString("Phone"),
                            Address = reader.GetString("Address"),
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

    private async Task<bool> UpdateUser(MySqlConnection connection, ResidenceInformationRequestDto residenceInformation, string username)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"UPDATE users SET Fullname=@Fullname, Gender=@Gender, Birthday=@Birthday, City=@City, District=@District, Ward=@Ward, Email=@Email, Phone=@Phone, Address=@Address WHERE Username=@Username;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@Fullname", residenceInformation.Fullname);
        command.Parameters.AddWithValue("@Gender", residenceInformation.Gender);
        command.Parameters.AddWithValue("@Birthday", residenceInformation.Birthday);
        command.Parameters.AddWithValue("@City", residenceInformation.City);
        command.Parameters.AddWithValue("@District", residenceInformation.District);
        command.Parameters.AddWithValue("@Ward", residenceInformation.Ward);
        command.Parameters.AddWithValue("@Email", residenceInformation.Email);
        command.Parameters.AddWithValue("@Phone", residenceInformation.Phone);
        command.Parameters.AddWithValue("@Address", residenceInformation.Address);
        command.Parameters.AddWithValue("@Username", username);

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

    // private async Task<bool> CreateResidenceInformation(MySqlConnection connection, int IdUsers, ResidenceInformationRequestDto residenceInformation)
    // {
    //     var rows_affected = 0;
    //     using var command = new MySqlCommand();
    //     command.Connection = connection;

    //     string queryString = @"INSERT INTO thongtincutru (IdUsers, IDToKhai, TrangThai) VALUES (@IdUsers, @IDToKhai, @TrangThai);
    //                             select last_insert_id();";

    //     command.CommandText = queryString;
    //     command.Parameters.AddWithValue("@IdUsers", IdUsers);
    //     command.Parameters.AddWithValue("@IdToKhai", residenceInformation.IDToKhai);
    //     command.Parameters.AddWithValue("@TrangThai", residenceInformation.TrangThai);

    //     try
    //     {
    //         rows_affected = await command.ExecuteNonQueryAsync();
    //         if (rows_affected > 0)
    //         {

    //             return true;
    //         }
    //         return false;

    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         return false;
    //     }
    // }
}