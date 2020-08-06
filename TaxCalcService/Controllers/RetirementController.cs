using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaxCalcService.Models;
using TaxCalcService.Models.DTO;

namespace TaxCalcService.Controllers
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

        // GET api/Retirement/Report?salary=20000&spending=19000&dob=1981-30-05&female=false&existingSavings=20000
        //https://sctaxcalcservice.azurewebsites.net/api/Retirement/Report?salary=100000&spending=40000&dob=1981-05-30&female=false&existingSavings=20000
        [HttpGet("Report")]
        public async Task<RetirementReportDto> GetReport([FromQuery] int salary, [FromQuery] int spending,
            [FromQuery] DateTime dob, [FromQuery] bool female, [FromQuery] int existingSavings, [FromQuery] int existingPension, 
            [FromQuery] decimal employerContribution, [FromQuery] decimal employeeContribution, [FromQuery] int? targetRetirementAge)
        {
            return await Task.Run(() => _retirement.RetirementReportFor(salary, spending, dob, female, existingSavings, 
                existingPension, employerContribution, employeeContribution, targetRetirementAge));
        }
    }
}