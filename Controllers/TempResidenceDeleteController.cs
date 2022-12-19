using System.Data;
using System.Data.Common;
using System.Security.Claims;
using FluentEmail.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Residence.DTOs;
using Residence.Entities;
using Residence.Services;

namespace Residence.Controllers;

public class TempResidenceDeleteController : BaseApiController
{
    private readonly ILogger<TempResidenceDeleteController> _logger;
    private readonly IConfiguration _configuration;
    private readonly EmailService<dynamic> _email;

    public TempResidenceDeleteController(ILogger<TempResidenceDeleteController> logger, IConfiguration configuration, IFluentEmail email)
    {
        _configuration = configuration;
        _logger = logger;
        _email = new EmailService<dynamic>(email);
    }
    

    [Authorize(Roles = "User")]
    [HttpPost("delete")]
    public async Task<ActionResult> PostDeleteResidenceRegister([FromBody] PostDeleteResidenceRegisterRequestDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var tempResidenceRegister = await GetTempResidenceRegister(connection, request.IdHoSoDangKiTamTru);
            if (tempResidenceRegister == null)
            {
                await connection.CloseAsync();
                return BadRequest();
            }
            var deleteTempResidenceRegisterFromDB = await GetDeleteTempResidenceRegister(connection, request.IdHoSoDangKiTamTru);
            if (deleteTempResidenceRegisterFromDB != null)
            {
                await connection.CloseAsync();
                return BadRequest("Delete already filed");
            }
            var deleteTempResidenceRegister = new DeleteTempResidenceRegister
            {
                IdHoSoDangKyTamTru = tempResidenceRegister.IdHoSoDkiTamtru,
                IdUsers = tempResidenceRegister.IdUsers,
                ThuTuc = tempResidenceRegister.ThuTuc,
                ThanhPho = tempResidenceRegister.ThanhPho,
                Quan = tempResidenceRegister.Quan,
                Phuong = tempResidenceRegister.Phuong,
                DiaChi = tempResidenceRegister.DiaChi,
                HoTenChuHo = tempResidenceRegister.HoTenChuHo,
                QuanHeVoiChuHo = tempResidenceRegister.QuanHeVoiChuHo,
                CMNDChuHo = tempResidenceRegister.CMNDChuHo,
                NoiDung = tempResidenceRegister.NoiDung,
                TamTruTuNgay = tempResidenceRegister.TamTruTuNgay,
                TamTruDenNgay = tempResidenceRegister.TamTruDenNgay,
                IdToKhai = request.IdToKhai,
                TrangThai = "Pending",
            };
            var result = await CreateDeleteTempResidenceRegister(connection, deleteTempResidenceRegister);
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

    [Authorize(Roles = "Administrator")]
    [HttpDelete("admin/delete")]
    public async Task<ActionResult> DeleteTempResidenceRegisterAdmin([FromBody] PostDeleteResidenceRegisterAdminRequestDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var updateResult = await UpdateDeleteTempResidenceRegisterStatus(connection, request.IdHoSoXoaGiaHan);
            if (updateResult == false)
            {
                await connection.CloseAsync();
                return BadRequest();
            }
            var result = await DeleteTempResidenceRegister(connection, request.IdHoSoDangKiTamTru);
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

    

    [Authorize(Roles = "Administrator")]
    [HttpGet("admin")]
    public async Task<ActionResult> GetForAdmin([FromQuery] PaginationRequestDto request)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var response = new PaginationResponseDto<TempResidenceRegisterResponseDto>();
            var count = await CountListTempResidenceRegisterDelete(connection);
            if (count == null || count == 0)
            {
                response.Data = new List<TempResidenceRegisterResponseDto>();
                response.Total = 0;
                await connection.CloseAsync();
                return Ok(response);
            }
            var list = await GetListTempResidenceRegisterDelete(connection, request);
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

    

    private async Task<List<TempResidenceRegisterResponseDto>?> GetListTempResidenceRegisterDelete(MySqlConnection connection, PaginationRequestDto request)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM xoadkitamtru ORDER BY IdHoSoXoaGiaHan DESC LIMIT @Limit OFFSET @Offset;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@Limit", request.PageSize);
        command.Parameters.AddWithValue("@Offset", request.PageSize * (request.PageNumber - 1));
        var response = new List<TempResidenceRegisterResponseDto>();
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var table = new TempResidenceRegisterResponseDto
                        {
                            IdHoSoDkiTamtru= reader.GetInt32("IdHoSoDkiTamtru"),
                            IdUsers = reader.GetInt32("IdUsers"),
                            ThuTuc = reader.GetString("ThuTuc"),
                            ThanhPho = reader.GetString("ThanhPho"),
                            Quan = reader.GetString("Quan"),
                            Phuong = reader.GetString("Phuong"),
                            DiaChi = reader.GetString("DiaChi"),
                            HoTenChuHo = reader.GetString("HoTenChuHo"),
                            QuanHeVoiChuHo = reader.GetString("QuanHeVoiChuHo"),
                            CMNDChuHo = reader.GetString("CMNDChuHo"),
                            NoiDung = reader.GetString("NoiDung"),
                            TamTruTuNgay = reader.GetDateTime("TamTruTuNgay"),
                            TamTruDenNgay = reader.GetDateTime("TamTruDenNgay"),
                            TrangThai = reader.GetString("TrangThai"),
                            IdToKhai = reader.GetInt32("IdToKhai"),
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

    private async Task<TempResidenceRegister?> GetTempResidenceRegister(MySqlConnection connection, int id)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM dangkytamtru WHERE IdHoSoDkiTamtru = @IdHoSoDkiTamtru;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@IdHoSoDkiTamtru", id);
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var table = new TempResidenceRegister
                        {
                            IdHoSoDkiTamtru= reader.GetInt32("IdHoSoDkiTamtru"),
                            IdUsers = reader.GetInt32("IdUsers"),
                            ThuTuc = reader.GetString("ThuTuc"),
                            ThanhPho = reader.GetString("ThanhPho"),
                            Quan = reader.GetString("Quan"),
                            Phuong = reader.GetString("Phuong"),
                            DiaChi = reader.GetString("DiaChi"),
                            HoTenChuHo = reader.GetString("HoTenChuHo"),
                            QuanHeVoiChuHo = reader.GetString("QuanHeVoiChuHo"),
                            CMNDChuHo = reader.GetString("CMNDChuHo"),
                            NoiDung = reader.GetString("NoiDung"),
                            TamTruTuNgay = reader.GetDateTime("TamTruTuNgay"),
                            TamTruDenNgay = reader.GetDateTime("TamTruDenNgay"),
                        };
                        return table;
                    }
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

    private async Task<TempResidenceRegister?> GetDeleteTempResidenceRegister(MySqlConnection connection, int id)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM xoadkitamtru WHERE IdHoSoDangKyTamTru = @IdHoSoDangKyTamTru;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@IdHoSoDangKyTamTru", id);
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var table = new TempResidenceRegister
                        {
                            IdHoSoDkiTamtru= reader.GetInt32("IdHoSoDangKyTamTru"),
                            IdUsers = reader.GetInt32("IdUsers"),
                            ThuTuc = reader.GetString("ThuTuc"),
                            ThanhPho = reader.GetString("ThanhPho"),
                            Quan = reader.GetString("Quan"),
                            Phuong = reader.GetString("Phuong"),
                            DiaChi = reader.GetString("DiaChi"),
                            HoTenChuHo = reader.GetString("HoTenChuHo"),
                            QuanHeVoiChuHo = reader.GetString("QuanHeVoiChuHo"),
                            CMNDChuHo = reader.GetString("CMNDChuHo"),
                            NoiDung = reader.GetString("NoiDung"),
                            TamTruTuNgay = reader.GetDateTime("TamTruTuNgay"),
                            TamTruDenNgay = reader.GetDateTime("TamTruDenNgay"),
                        };
                        return table;
                    }
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

    private async Task<bool> UpdateDeleteTempResidenceRegisterStatus(MySqlConnection connection, int id)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"UPDATE xoadkitamtru SET TrangThai=@TrangThai WHERE IdHoSoXoaGiaHan=@Id;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@TrangThai", "Approved");
        command.Parameters.AddWithValue("@Id", id);

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

    private async Task<int?> CountListTempResidenceRegisterDelete(MySqlConnection connection)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT COUNT(IdHoSoXoaGiaHan	) as NumberOfTempResidenceRegisterDelete FROM xoadkitamtru;";

        command.CommandText = queryString;
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var count = reader.GetInt32("NumberOfTempResidenceRegisterDelete");
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

    private async Task<bool> CreateDeleteTempResidenceRegister(MySqlConnection connection, DeleteTempResidenceRegister deleteTempResidenceRegister)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"INSERT INTO xoadkitamtru (IdHoSoDangKyTamTru, IdUsers, ThuTuc, ThanhPho, Quan, Phuong, DiaChi, HoTenChuHo, QuanHeVoiChuHo, CMNDChuHo, NoiDung, TamTruTuNgay, TamTruDenNgay, IDToKhai, TrangThai) VALUES (@IdHoSoDangKyTamTru, @IdUsers, @ThuTuc, @ThanhPho, @Quan, @Phuong, @DiaChi, @HoTenChuHo, @QuanHeVoiChuHo, @CMNDChuHo, @NoiDung, @TamTruTuNgay, @TamTruDenNgay, @IDToKhai, @TrangThai);
                                select last_insert_id();";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@IdHoSoDangKyTamTru", deleteTempResidenceRegister.IdHoSoDangKyTamTru);
        command.Parameters.AddWithValue("@IdUsers", deleteTempResidenceRegister.IdUsers);
        command.Parameters.AddWithValue("@ThuTuc", deleteTempResidenceRegister.ThuTuc);
        command.Parameters.AddWithValue("@ThanhPho", deleteTempResidenceRegister.ThanhPho);
        command.Parameters.AddWithValue("@Quan", deleteTempResidenceRegister.Quan);
        command.Parameters.AddWithValue("@Phuong", deleteTempResidenceRegister.Phuong);
        command.Parameters.AddWithValue("@DiaChi", deleteTempResidenceRegister.DiaChi);
        command.Parameters.AddWithValue("@HoTenChuHo", deleteTempResidenceRegister.HoTenChuHo);
        command.Parameters.AddWithValue("@QuanHeVoiChuHo", deleteTempResidenceRegister.QuanHeVoiChuHo);
        command.Parameters.AddWithValue("@CMNDChuHo", deleteTempResidenceRegister.CMNDChuHo);
        command.Parameters.AddWithValue("@NoiDung", deleteTempResidenceRegister.NoiDung);
        command.Parameters.AddWithValue("@TamTruTuNgay", deleteTempResidenceRegister.TamTruTuNgay);
        command.Parameters.AddWithValue("@TamTruDenNgay", deleteTempResidenceRegister.TamTruDenNgay);
        command.Parameters.AddWithValue("@IdToKhai", deleteTempResidenceRegister.IdToKhai);
        command.Parameters.AddWithValue("@TrangThai", deleteTempResidenceRegister.TrangThai);

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

    private async Task<bool> DeleteTempResidenceRegister(MySqlConnection connection, int IdHoSoDangKyTamTru)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"DELETE FROM dangkytamtru WHERE IdHoSoDkiTamtru = @IdHoSoDangKyTamTru";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@IdHoSoDangKyTamTru", IdHoSoDangKyTamTru);

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