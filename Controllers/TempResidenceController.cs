using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Residence.DTOs;
using Residence.Entities;

namespace Residence.Controllers;

public class TempResidenceController : BaseApiController
{
    private readonly ILogger<TempResidenceController> _logger;
    private readonly IConfiguration _configuration;

    public TempResidenceController(ILogger<TempResidenceController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }
    [Authorize(Roles = "User")]
    [HttpPost]
    public async Task<ActionResult> Post([FromBody] TempResidenceRegisterDto tempResidenceRegisterDto)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var newTempResidenceRegister = new TempResidenceRegister
            {
                IdUsers = tempResidenceRegisterDto.IdUsers,
                ThuTuc = tempResidenceRegisterDto.ThuTuc,
                ThanhPho = tempResidenceRegisterDto.ThanhPho,
                Quan = tempResidenceRegisterDto.Quan,
                Phuong = tempResidenceRegisterDto.Phuong,
                DiaChi = tempResidenceRegisterDto.DiaChi,
                HoTenChuHo = tempResidenceRegisterDto.HoTenChuHo,
                QuanHeVoiChuHo = tempResidenceRegisterDto.QuanHeVoiChuHo,
                CMNDChuHo = tempResidenceRegisterDto.CMNDChuHo,
                NoiDung = tempResidenceRegisterDto.NoiDung,
                TamTruTuNgay = tempResidenceRegisterDto.TamTruTuNgay,
                TamTruDenNgay = tempResidenceRegisterDto.TamTruDenNgay,
                IdToKhai = tempResidenceRegisterDto.IdToKhai,
                TrangThai = tempResidenceRegisterDto.TrangThai,
            };

            var result = await CreateTempResidenceRegister(connection, newTempResidenceRegister);
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
    [Authorize(Roles = "User")]
    [HttpPatch]
    public async Task<ActionResult> Patch([FromBody] TempResidenceRegisterRequestDto tempResidenceRegister)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var result = await UpdateTempResidenceRegister(connection, tempResidenceRegister);
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
    [HttpPatch("admin")]
    public async Task<ActionResult> PatchForAdmin([FromBody] TempResidenceRegisterRequestAdminDto tempResidenceRegister)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var result = await UpdateTempResidenceRegisterStatus(connection, tempResidenceRegister);
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

    private async Task<bool> CreateTempResidenceRegister(MySqlConnection connection, TempResidenceRegister tempResidenceRegister)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"INSERT INTO dangkitamtru (IdUsers, ThuTuc, ThanhPho, Quan, Phuong, DiaChi, HoTenChuHo, QuanHeVoiChuHo, CMNDChuHo, NoiDung, TamTruTuNgay, TamTruDenNgay, IDToKhai, TrangThai) VALUES (@IdUsers, @ThuTuc, @ThanhPho, @Quan, @Phuong, @DiaChi, @HoTenChuHo, @QuanHeVoiChuHo, @CMNDChuHo, @NoiDung, @TamTruTuNgay, @TamTruDenNgay, @IDToKhai, @TrangThai);
                                select last_insert_id();";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@IdUsers", tempResidenceRegister.IdUsers);
        command.Parameters.AddWithValue("@ThuTuc", tempResidenceRegister.ThuTuc);
        command.Parameters.AddWithValue("@ThanhPho", tempResidenceRegister.ThanhPho);
        command.Parameters.AddWithValue("@Quan", tempResidenceRegister.Quan);
        command.Parameters.AddWithValue("@Phuong", tempResidenceRegister.Phuong);
        command.Parameters.AddWithValue("@DiaChi", tempResidenceRegister.DiaChi);
        command.Parameters.AddWithValue("@HoTenChuHo", tempResidenceRegister.HoTenChuHo);
        command.Parameters.AddWithValue("@QuanHeVoiChuHo", tempResidenceRegister.QuanHeVoiChuHo);
        command.Parameters.AddWithValue("@CMNDChuHo", tempResidenceRegister.CMNDChuHo);
        command.Parameters.AddWithValue("@NoiDung", tempResidenceRegister.NoiDung);
        command.Parameters.AddWithValue("@TamTruTuNgay", tempResidenceRegister.TamTruTuNgay);
        command.Parameters.AddWithValue("@TamTruDenNgay", tempResidenceRegister.TamTruDenNgay);
        command.Parameters.AddWithValue("@IdToKhai", tempResidenceRegister.IdToKhai);
        command.Parameters.AddWithValue("@TrangThai", tempResidenceRegister.TrangThai);

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

    private async Task<bool> UpdateTempResidenceRegister(MySqlConnection connection, TempResidenceRegisterRequestDto tempResidenceRegister)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"UPDATE dangkitamtru SET ThanhPho=@ThanhPho, Quan=@Quan, Phuong=@Phuong, DiaChi=@DiaChi, TamTruTuNgay=@TamTruTuNgay, TamTruDenNgay=@TamTruDenNgay WHERE IdHoSoDkiTamtru=@Id;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@ThanhPho", tempResidenceRegister.TamTruThanhPho);
        command.Parameters.AddWithValue("@Quan", tempResidenceRegister.TamTruQuan);
        command.Parameters.AddWithValue("@Phuong", tempResidenceRegister.TamTruPhuong);
        command.Parameters.AddWithValue("@DiaChi", tempResidenceRegister.TamTruDiaChi);
        command.Parameters.AddWithValue("@TamTruTuNgay", tempResidenceRegister.TamTruTuNgay);
        command.Parameters.AddWithValue("@TamTruDenNgay", tempResidenceRegister.TamTruDenNgay);
        command.Parameters.AddWithValue("@Id", tempResidenceRegister.IdHoSoDkiTamtru);

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

    private async Task<bool> UpdateTempResidenceRegisterStatus(MySqlConnection connection, TempResidenceRegisterRequestAdminDto tempResidenceRegister)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"UPDATE dangkitamtru SET TrangThai=@TrangThai WHERE IdHoSoDkiTamtru=@Id;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@TrangThai", tempResidenceRegister.TrangThai);
        command.Parameters.AddWithValue("@Id", tempResidenceRegister.IdHoSoDkiTamtru);

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