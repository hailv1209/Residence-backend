using System.Data;
using System.Data.Common;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Residence.DTOs;
using Residence.Entities;
using Residence.Enums;
using RestSharp;

namespace Residence.Controllers;

public class ProvinceController : BaseApiController
{
    private readonly ILogger<ProvinceController> _logger;
    private readonly IConfiguration _configuration;

    public ProvinceController(ILogger<ProvinceController> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var client = new RestClient("https://provinces.open-api.vn/api/p/");
        var request = new RestRequest("", Method.Get);
        var response = await client.ExecuteAsync(request);
        var result = JsonSerializer.Deserialize<IEnumerable<Province>>(response.Content!);
        return Ok(result);
    }

    [HttpGet("districts/{code}")]
    public async Task<ActionResult> GetDistricts(string code)
    {
        var client = new RestClient($"https://provinces.open-api.vn/api/p/{code}?depth=2");
        var request = new RestRequest("", Method.Get);
        var response = await client.ExecuteAsync(request);
        var result = JsonSerializer.Deserialize<Province>(response.Content!);
        return Ok(result);
    }

    [HttpGet("districts/{code}/wards")]
    public async Task<ActionResult> GetWards(string code)
    {
        var client = new RestClient($"https://provinces.open-api.vn/api/d/{code}?depth=2");
        var request = new RestRequest("", Method.Get);
        var response = await client.ExecuteAsync(request);
        var result = JsonSerializer.Deserialize<District>(response.Content!);
        return Ok(result);
    }

}