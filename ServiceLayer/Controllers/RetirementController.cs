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
        
        [HttpPost("Report")]
        public async Task<RetirementReportDto> Report([FromBody] string body)
        {
            var options = new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
            var request = JsonSerializer.Deserialize<RetirementReportRequestDto>(body, options);
            return await _retirement.RetirementReportForAsync(request.TargetRetirementAge, request.EmergencyFund, request.SpendingSteps, request.Persons);
        }
    }
}