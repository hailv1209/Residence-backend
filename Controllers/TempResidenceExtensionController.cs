using System.Data;
using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Residence.DTOs;
using Residence.Entities;

namespace Residence.Controllers;

[Authorize]
public class TempResidenceExtensionController : BaseApiController
{
    private readonly ILogger<TempResidenceExtensionController> _logger;
    private readonly IConfiguration _configuration;

    public TempResidenceExtensionController(ILogger<TempResidenceExtensionController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }
    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<ActionResult<TempResidenceExtensionResponseDto>> Get()
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

            var residenceRegister = await GetTempResidenceRegister(connection, userFromDB.IdUsers);

            if (residenceRegister == null)
            {
                await connection.CloseAsync();
                return NotFound("Residence register not found");
            }

            var result = new TempResidenceExtensionResponseDto
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
                TamTruThanhPho = residenceRegister.ThanhPho,
                TamTruQuan = residenceRegister.Quan,
                TamTruPhuong = residenceRegister.Phuong,
                TamTruDiaChi = residenceRegister.DiaChi,
                TamTruTuNgay = residenceRegister.TamTruTuNgay,
                TamTruDenNgay = residenceRegister.TamTruDenNgay,

            };

            return result;
        }
        await connection.CloseAsync();
        return BadRequest();
    }
    [Authorize(Roles = "User")]
    [HttpPost]
    public async Task<ActionResult<TempResidenceExtensionResponseDto>> Post([FromBody] TempResidenceExtensionRequestDto tempResidenceExtension)
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

            var residenceRegisterResult = await UpdateTempResidenceRegister(connection, tempResidenceExtension);

            if (!residenceRegisterResult)
            {
                await connection.CloseAsync();
                return BadRequest("Cannot update residence register");
            }

            var tempResidenceExtensionResult = await CreateTempResidenceExtension(connection, userFromDB.IdUsers, tempResidenceExtension);

            if (tempResidenceExtensionResult)
            {
                await connection.CloseAsync();
                return Ok("Updated successfully");
            }

            await connection.CloseAsync();
            return BadRequest("Cannot create residence extension");

        }
        await connection.CloseAsync();
        return BadRequest();
    }
    [Authorize(Roles = "Administrator")]
    [HttpPatch("admin")]
    public async Task<ActionResult<bool>> Patch(TempResidenceExtensionRequestAdminDto tempResidenceExtension)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var tempResidenceExtensionResult = await UpdateTempResidenceExtensionStatus(connection, tempResidenceExtension);

            if (tempResidenceExtensionResult)
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

    private async Task<TempResidenceRegister?> GetTempResidenceRegister(MySqlConnection connection, int IdUsers)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM dangkitamtru WHERE IdUsers = @IdUsers;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@IdUsers", IdUsers);
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var tempResidenceRegister = new TempResidenceRegister
                        {
                            IdHoSoDkiTamtru = reader.GetInt32("IdHoSoDkiTamtru"),
                            IdUsers = reader.GetInt32("IdUsers"),
                            ThanhPho = reader.GetString("ThanhPho"),
                            Quan = reader.GetString("Quan"),
                            Phuong = reader.GetString("Phuong"),
                            DiaChi = reader.GetString("DiaChi"),
                            TamTruTuNgay = (DateTime)reader["TamTruTuNgay"],
                            TamTruDenNgay = (DateTime)reader["TamTruDenNgay"],
                        };
                        return tempResidenceRegister;
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

    private async Task<bool> UpdateTempResidenceRegister(MySqlConnection connection, TempResidenceExtensionRequestDto tempResidenceExtension)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"UPDATE dangkitamtru SET ThanhPho=@ThanhPho, Quan=@Quan, Phuong=@Phuong, DiaChi=@DiaChi, TamTruTuNgay=@TamTruTuNgay, TamTruDenNgay=@TamTruDenNgay WHERE IdHoSoDkiTamtru=@Id;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@ThanhPho", tempResidenceExtension.TamTruThanhPho);
        command.Parameters.AddWithValue("@Quan", tempResidenceExtension.TamTruQuan);
        command.Parameters.AddWithValue("@Phuong", tempResidenceExtension.TamTruPhuong);
        command.Parameters.AddWithValue("@DiaChi", tempResidenceExtension.TamTruDiaChi);
        command.Parameters.AddWithValue("@TamTruTuNgay", tempResidenceExtension.TamTruTuNgay);
        command.Parameters.AddWithValue("@TamTruDenNgay", tempResidenceExtension.TamTruDenNgay);
        command.Parameters.AddWithValue("@Id", tempResidenceExtension.IdHoSoDkiTamtru);

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

    private async Task<bool> CreateTempResidenceExtension(MySqlConnection connection, int IdUsers, TempResidenceExtensionRequestDto tempResidenceExtension)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"INSERT INTO giahantamtru (IdUsers, ThuTuc, IDToKhai, TrangThai) VALUES (@IdUsers, @ThuTuc, @IDToKhai, @TrangThai);
                                select last_insert_id();";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@IdUsers", IdUsers);
        command.Parameters.AddWithValue("@IdToKhai", tempResidenceExtension.IDToKhai);
        command.Parameters.AddWithValue("@ThuTuc", tempResidenceExtension.ThuTuc);
        command.Parameters.AddWithValue("@TrangThai", tempResidenceExtension.TrangThai);

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

    private async Task<bool> UpdateTempResidenceExtensionStatus(MySqlConnection connection, TempResidenceExtensionRequestAdminDto tempResidenceExtension)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"UPDATE giahantamtru SET TrangThai=@TrangThai WHERE IdHosotamtru=@Id;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@TrangThai", tempResidenceExtension.TrangThai);
        command.Parameters.AddWithValue("@Id", tempResidenceExtension.IdHosotamtru);

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