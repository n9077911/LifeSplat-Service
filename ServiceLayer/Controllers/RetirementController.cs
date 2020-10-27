using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Models;
using ServiceLayer.Models.DTO;
namespace ServiceLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RetirementController : ControllerBase
    {
        private readonly IRetirementDomainInterface _retirement;

        public RetirementController(IRetirementDomainInterface retirement)
        {
            _retirement = retirement;
        }

        [HttpGet("Report")]
        public async Task<RetirementReportDto> Report([FromQuery] string criteria)
        {
            var options = new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
            var request = JsonSerializer.Deserialize<RetirementReportRequestDto>(criteria, options);
            return await _retirement.RetirementReportForAsync(request);
        }
    }
}