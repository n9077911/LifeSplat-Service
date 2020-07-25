using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaxCalcService.Models;
using TaxCalcService.Models.DTO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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

        // GET api/Retirement/Report?salary=20000&spending=19000&dob=1981-30-05
        [HttpGet("Report")]
        public RetirementReportDto GetReport([FromQuery]int salary, [FromQuery]int spending, [FromQuery] DateTime dob)
        {
            return _retirement.RetirementReportFor(salary, spending, dob);
        }

    }
}
