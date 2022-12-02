using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Residence.DTOs;
using Residence.Entities;
using Residence.Services;

namespace Residence.Controllers;

public class CitizenDeclarationController : BaseApiController
{
    private readonly ILogger<CitizenDeclarationController> _logger;
    private readonly AWS3Service _service;
    private readonly IConfiguration _configuration;

    public CitizenDeclarationController(ILogger<CitizenDeclarationController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
        _service = new AWS3Service(configuration);
    }

    [Authorize(Roles = "User")]
    [HttpPost]
    public async Task<ActionResult<CitizenDeclarationResponseDto>> UploadFile([FromForm] DeclarationRequestDto declarationRequest)
    {
        var randomStr = Convert.ToString(Convert.ToInt64(DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds));
        string originalFileName = Path.GetFileName(declarationRequest.File!.FileName);
        string keyFileName = originalFileName.Replace(" ", "-").Replace("_", "-").ToLower();
        string key = _configuration["AWSServiceSettings:CitizenDeclarationFolder"];
        key += @"/" + $"CitizenDeclaration_{randomStr}_{keyFileName}";
        var result = await _service.UploadFileAsync(declarationRequest.File!, key);
        if (result)
        {
            var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
            var connection = new MySqlConnection(sqlconnectstring);
            await connection.OpenAsync();
            if (connection.State == ConnectionState.Open)
            {
                var citizenDeclarationResult = await CreateCitizenDeclaration(connection, key, declarationRequest);
                if (citizenDeclarationResult)
                {
                    var citizenDeclaration = await GetCitizenDeclaration(connection, key);
                    await connection.CloseAsync();
                    var sampDeclarationToReturn = new CitizenDeclarationResponseDto
                    {
                        IDToKhai = citizenDeclaration!.IDToKhai,
                        TenGiayTo = citizenDeclaration!.TenGiayTo
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
    [HttpGet("download/{IDToKhai}")]
    public async Task<ActionResult> DownloadFile(int IDToKhai)
    {
        var sqlconnectstring = _configuration.GetConnectionString("DefaultConnection");
        var connection = new MySqlConnection(sqlconnectstring);
        await connection.OpenAsync();
        if (connection.State == ConnectionState.Open)
        {
            var citizenDeclaration = await GetCitizenDeclarationById(connection, IDToKhai);
            if (citizenDeclaration == null)
            {
                await connection.CloseAsync();
                return NotFound("File not found");
            }

            var document = _service.DownloadFileAsync(citizenDeclaration.File!).Result;

            var fileType = citizenDeclaration.File!.Split('.').LastOrDefault();

            await connection.CloseAsync();
            return File(document, "application/octet-stream", $"{citizenDeclaration.TenGiayTo}.{fileType}");
        }
        await connection.CloseAsync();
        return BadRequest();
    }

    private async Task<bool> CreateCitizenDeclaration(MySqlConnection connection, string file, DeclarationRequestDto declarationRequest)
    {
        var rows_affected = 0;
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"INSERT INTO tokhaicongdan (TenGiayTo, File, UploadedAt) VALUES (@TenGiayTo, @File, @UploadedAt);
                                select last_insert_id();";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@TenGiayTo", declarationRequest.TenGiayTo);
        command.Parameters.AddWithValue("@File", file);
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

    private async Task<CitizenDeclaration?> GetCitizenDeclaration(MySqlConnection connection, string file)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM tokhaicongdan WHERE File = @file;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@file", file);
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var citizenDeclaration = new CitizenDeclaration
                        {
                            IDToKhai = reader.GetInt32("IDToKhai"),
                            TenGiayTo = reader.GetString("TenGiayTo"),
                            File = reader.GetString("File"),
                        };
                        return citizenDeclaration;
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

    private async Task<CitizenDeclaration?> GetCitizenDeclarationById(MySqlConnection connection, int IDToKhai)
    {
        using var command = new MySqlCommand();
        command.Connection = connection;

        string queryString = @"SELECT * FROM tokhaicongdan WHERE IDToKhai = @IDToKhai;";

        command.CommandText = queryString;
        command.Parameters.AddWithValue("@IDToKhai", IDToKhai);
        try
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var citizenDeclaration = new CitizenDeclaration
                        {
                            IDToKhai = reader.GetInt32("IDToKhai"),
                            TenGiayTo = reader.GetString("TenGiayTo"),
                            File = reader.GetString("File"),
                        };
                        return citizenDeclaration;
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