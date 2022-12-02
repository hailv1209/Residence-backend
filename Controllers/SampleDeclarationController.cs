using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Residence.DTOs;
using Residence.Entities;
using Residence.Services;

namespace Residence.Controllers;

public class SampleDeclarationController : BaseApiController
{
    private readonly ILogger<SampleDeclarationController> _logger;
    private readonly AWS3Service _service;
    private readonly IConfiguration _configuration;

    public SampleDeclarationController(ILogger<SampleDeclarationController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
        _service = new AWS3Service(configuration);
    }
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    public async Task<ActionResult<SampleDeclarationResponseDto>> UploadFile([FromForm] DeclarationRequestDto declarationRequest)
    {
        var randomStr = Convert.ToString(Convert.ToInt64(DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds));
        string originalFileName = Path.GetFileName(declarationRequest.File!.FileName);
        string keyFileName = originalFileName.Replace(" ", "-").Replace("_", "-").ToLower();
        string key = _configuration["AWSServiceSettings:SampleDeclarationFolder"];
        key += @"/" + $"SampleDeclaration_{randomStr}_{keyFileName}";
        var result = await _service.UploadFileAsync(declarationRequest.File!, key);
        if (result)
        {
            var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
            var connection = new MySqlConnection(sqlconnectstring);
            await connection.OpenAsync();
            if (connection.State == ConnectionState.Open)
            {
                var sampleDeclarationResult = await CreateSampleDeclaration(connection, key, declarationRequest);
                if (sampleDeclarationResult)
                {
                    var sampleDeclaration = await GetSampleDeclaration(connection, key);
                    await connection.CloseAsync();
                    var sampDeclarationToReturn = new SampleDeclarationResponseDto
                    {
                        IDToKhaiMau = sampleDeclaration!.IDToKhaiMau,
                        TenGiayTo = sampleDeclaration!.TenGiayTo
                    };
                    return Ok(sampDeclarationToReturn);
                }
            }
            await connection.CloseAsync();
            return BadRequest();
        }
        return BadRequest();
    }
    [Authorize]
    [HttpGet("download/{IDToKhaiMau}")]
    public async Task<ActionResult> DownloadFile(int IDToKhaiMau)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var sampleDeclaration = await GetSampleDeclarationById(connection, IDToKhaiMau);
            if (sampleDeclaration == null)
            {
                await connection.CloseAsync();
                return NotFound("File not found");
            }

            var document = _service.DownloadFileAsync(sampleDeclaration.FileMau!).Result;

            var fileType = sampleDeclaration.FileMau!.Split('.').LastOrDefault();

            await connection.CloseAsync();
            return File(document, "application/octet-stream", $"{sampleDeclaration.TenGiayTo}.{fileType}");
        }
        await connection.CloseAsync();
        return BadRequest();
    }

    private async Task<bool> CreateSampleDeclaration(MySqlConnection connection, string fileMau, DeclarationRequestDto declarationRequest)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"INSERT INTO tokhaimau (TenGiayTo, FileMau, UploadedAt) VALUES (@TenGiayTo, @FileMau, @UploadedAt);
                                select last_insert_id();";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@TenGiayTo", declarationRequest.TenGiayTo);
        command.Parameters.AddWithValue("@FileMau", fileMau);
        command.Parameters.AddWithValue("@UploadedAt", DateTime.UtcNow);

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

    private async Task<SampleDeclaration?> GetSampleDeclaration(MySqlConnection connection, string fileMau)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM tokhaimau WHERE FileMau = @fileMau;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@fileMau", fileMau);
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var sampleDeclaration = new SampleDeclaration
                        {
                            IDToKhaiMau = reader.GetInt32("IDToKhaiMau"),
                            TenGiayTo = reader.GetString("TenGiayTo"),
                            FileMau = reader.GetString("FileMau"),
                        };
                        return sampleDeclaration;
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

    private async Task<SampleDeclaration?> GetSampleDeclarationById(MySqlConnection connection, int IDToKhaiMau)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM tokhaimau WHERE IDToKhaiMau = @IDToKhaiMau;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@IDToKhaiMau", IDToKhaiMau);
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var sampleDeclaration = new SampleDeclaration
                        {
                            IDToKhaiMau = reader.GetInt32("IDToKhaiMau"),
                            TenGiayTo = reader.GetString("TenGiayTo"),
                            FileMau = reader.GetString("FileMau"),
                        };
                        return sampleDeclaration;
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