using System.Data;
using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Residence.DTOs;
using Residence.Entities;
using Residence.Enums;

namespace Residence.Controllers;

[Authorize(Roles = "Administrator")]
public class AnswerController : BaseApiController
{
    private readonly ILogger<AnswerController> _logger;
    private readonly IConfiguration _configuration;

    public AnswerController(ILogger<AnswerController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<bool>> Post([FromBody] AnswerRequestDto answerRequest)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var result = await CreateAnswer(connection, answerRequest);
            if (result)
            {
                await connection.CloseAsync();
                return Ok(true);
            }
        }
        await connection.CloseAsync();
        return BadRequest();
    }

    private async Task<bool> CreateAnswer(MySqlConnection connection, AnswerRequestDto answerRequest)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"INSERT INTO cautraloi (CauTraLoi, IdCauHoi, CreatedAt, UpdatedAt) VALUES (@CauTraLoi, @IdCauHoi, @CreatedAt, @UpdatedAt);
                                select last_insert_id();";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@CauTraLoi", answerRequest.CauTraLoi);
        command.Parameters.AddWithValue("@IdCauHoi", answerRequest.IdCauHoi);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

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

}