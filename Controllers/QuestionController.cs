using System.Data;
using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Residence.DTOs;
using Residence.Entities;

namespace Residence.Controllers;


public class QuestionController : BaseApiController
{
    private readonly ILogger<QuestionController> _logger;
    private readonly IConfiguration _configuration;

    public QuestionController(ILogger<QuestionController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "User")]
public async Task<ActionResult<bool>> Post([FromBody] QuestionRequestDto questionRequest)
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

            var result = await CreateQuestion(connection, userFromDB.IdUsers, questionRequest);
            if (result)
            {
                await connection.CloseAsync();
                return Ok(true);
            }
        }
        await connection.CloseAsync();
        return BadRequest();
    }

    [HttpGet("questionnaire")]
    [Authorize(Roles = "User")]
public async Task<ActionResult<List<QuestionAnswerDto>>> GetQuestionnaire()
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

            var result = await GetQuestionnaire(connection, userFromDB.IdUsers);
            if (result != null)
            {
                await connection.CloseAsync();
                return Ok(result);
            }
        }
        await connection.CloseAsync();
        return BadRequest();
    }

    [HttpGet("admin/questionnaire")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<PaginationResponseDto<QuestionAnswerDto>>> GetQuestionnaireForAdmin([FromQuery] PaginationRequestDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var response = new PaginationResponseDto<QuestionAnswerDto>();
            var count = await CountQuestionnaire(connection);
            if (count == null || count == 0)
            {
                response.Data = new List<QuestionAnswerDto>();
                response.Total = 0;
                await connection.CloseAsync();
                return Ok(response);
            }
            var list = await GetQuestionnaireForAdmin(connection, request);
            if (list == null)
            {
                await connection.CloseAsync();
                return BadRequest();
            }
            response.Data = list;
            response.Total = (int)count;
            await connection.CloseAsync();
            return Ok(response);
        }
        return BadRequest();
    }

    private async Task<bool> CreateQuestion(MySqlConnection connection, int IdUsers, QuestionRequestDto questionRequest)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"INSERT INTO cauhoi (CauHoi, IdUsers, CreatedAt, UpdatedAt) VALUES (@CauHoi, @IdUsers, @CreatedAt, @UpdatedAt);
                                select last_insert_id();";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@CauHoi", questionRequest.CauHoi);
        command.Parameters.AddWithValue("@IdUsers", IdUsers);
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

    private async Task<List<QuestionAnswerDto>?> GetQuestionnaire(MySqlConnection connection, int IdUsers)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT cauhoi.IdCauHoi, cauhoi.CauHoi, cauhoi.UpdatedAt as CauHoiUpdatedAt, cautraloi.IdTraLoi, cautraloi.CauTraLoi, cautraloi.UpdatedAt as CauTraLoiUpdatedAt FROM cauhoi LEFT JOIN cautraloi ON cauhoi.IdCauHoi = cautraloi.IdCauHoi WHERE IdUsers = @IdUsers;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@IdUsers", IdUsers);
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    var list = new List<QuestionAnswerDto>();
                    while (reader.Read())
                    {
                        var questionAnswer = new QuestionAnswerDto
                        {
                            IdCauHoi = reader.GetInt32("IdCauHoi"),
                            IdTraLoi = reader["IdTraLoi"] != DBNull.Value ? reader.GetInt32("IdTraLoi") : null,
                            CauHoi = reader.GetString("CauHoi"),
                            CauTraLoi = reader["CauTraLoi"] != DBNull.Value ? reader.GetString("CauTraLoi") : null,
                            CauHoiUpdatedAt = (DateTime)reader["CauHoiUpdatedAt"],
                            CauTraLoiUpdatedAt = reader["CauTraLoiUpdatedAt"] != DBNull.Value ? (DateTime)reader["CauTraLoiUpdatedAt"] : null,
                        };
                        list.Add(questionAnswer);
                    }
                    return list;
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

    private async Task<List<QuestionAnswerDto>?> GetQuestionnaireForAdmin(MySqlConnection connection, PaginationRequestDto request)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT cauhoi.IdCauHoi, cauhoi.CauHoi, cauhoi.UpdatedAt as CauHoiUpdatedAt, cautraloi.IdTraLoi, cautraloi.CauTraLoi, cautraloi.UpdatedAt as CauTraLoiUpdatedAt FROM cauhoi LEFT JOIN cautraloi ON cauhoi.IdCauHoi = cautraloi.IdCauHoi ORDER BY cauhoi.IdCauHoi DESC LIMIT @Limit OFFSET @Offset;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@Limit", request.PageSize);
        command.Parameters.AddWithValue("@Offset", request.PageSize * (request.PageNumber - 1));
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    var list = new List<QuestionAnswerDto>();
                    while (reader.Read())
                    {
                        var questionAnswer = new QuestionAnswerDto
                        {
                            IdCauHoi = reader.GetInt32("IdCauHoi"),
                            IdTraLoi = reader["IdTraLoi"] != DBNull.Value ? reader.GetInt32("IdTraLoi") : null,
                            CauHoi = reader.GetString("CauHoi"),
                            CauTraLoi = reader["CauTraLoi"] != DBNull.Value ? reader.GetString("CauTraLoi") : null,
                            CauHoiUpdatedAt = (DateTime)reader["CauHoiUpdatedAt"],
                            CauTraLoiUpdatedAt = reader["CauTraLoiUpdatedAt"] != DBNull.Value ? (DateTime)reader["CauTraLoiUpdatedAt"] : null,
                        };
                        list.Add(questionAnswer);
                    }
                    return list;
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

    private async Task<int?> CountQuestionnaire(MySqlConnection connection)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT COUNT(IdCauHoi) as NumberOfQuestions FROM cauhoi;";

        command.CommandText = queryString;
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var count = reader.GetInt32("NumberOfQuestions");
                        return count;
                    }
                }
                return 0;
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

}