using System.Data;
using System.Data.Common;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Residence.DTOs;
using Residence.Entities;
using RestSharp;

namespace Residence.Controllers;



public class ResidenceInformationController : BaseApiController
{
    private readonly ILogger<ResidenceInformationController> _logger;
    private readonly IConfiguration _configuration;

    public ResidenceInformationController(ILogger<ResidenceInformationController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [Authorize(Roles = "User")]
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

    [Authorize(Roles = "User")]
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


    [Authorize(Roles = "Administrator")]
    [HttpGet("admin")]
    public async Task<ActionResult> GetListUserAdmin ([FromQuery] PaginationRequestDto request) 
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var response = new PaginationResponseDto<ResidenceInformationAdminRequestDto>();
            var count = await CountListResidence(connection);
            if (count == null || count == 0)
            {
                response.Data = new List<ResidenceInformationAdminRequestDto>();
                response.Total = 0;
                await connection.CloseAsync();
                return Ok(response);
            }
            var list = await GetListResidence(connection, request);
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

   private async Task<List<ResidenceInformationAdminRequestDto>?> GetListResidence(MySqlConnection connection, PaginationRequestDto request)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM users ORDER BY IdUsers DESC LIMIT @Limit OFFSET @Offset;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@Limit", request.PageSize);
        command.Parameters.AddWithValue("@Offset", request.PageSize * (request.PageNumber - 1));
        var provinces = await GetProvinces();
        var response = new List<ResidenceInformationAdminRequestDto>();
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var table = new ResidenceInformationAdminRequestDto
                        {
                             IdUsers = reader.GetInt32("IdUsers"),
                            Gender = reader.GetString("Gender"),
                            Fullname = reader.GetString("Fullname"),
                            Birthday = (DateTime)reader["Birthday"],
                            City = provinces.FirstOrDefault(province => province.Code!.ToString() == reader.GetString("City"))!.Name,
                            District = await GetDistrict(reader.GetString("District")),
                            Ward = await GetWard(reader.GetString("Ward")),
                            Email = reader.GetString("Email"),
                            Phone = reader.GetString("Phone"),
                            Address = reader.GetString("Address"),
                        };
                        response.Add(table);
                    }
                    return response;
                }
                return response;
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    private async Task<int?> CountListResidence(MySqlConnection connection)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT COUNT(IdUsers	) as NumberOfResidence FROM users;";

        command.CommandText = queryString;
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var count = reader.GetInt32("NumberOfResidence");
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

    private async Task<List<Province>> GetProvinces()
    {
        var client = new RestClient("https://provinces.open-api.vn/api/p/");
        var request = new RestRequest("", Method.Get);
        var response = await client.ExecuteAsync(request);
        var result = JsonSerializer.Deserialize<List<Province>>(response.Content!);
        return result!;
    }

    private async Task<string> GetDistrict(string districtCode)
    {
        var client = new RestClient($"https://provinces.open-api.vn/api/d/{districtCode}");
        var request = new RestRequest("", Method.Get);
        var response = await client.ExecuteAsync(request);
        var result = JsonSerializer.Deserialize<District>(response.Content!);
        return result!.Name!;
    }

    private async Task<string> GetWard(string wardCode)
    {
        var client = new RestClient($"https://provinces.open-api.vn/api/w/{wardCode}");
        var request = new RestRequest("", Method.Get);
        var response = await client.ExecuteAsync(request);
        var result = JsonSerializer.Deserialize<Ward>(response.Content!);
        return result!.Name!;
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