using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaxCalcService.Models;
using TaxCalcService.Models.DTO;

namespace TaxCalcService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxResultsController : ControllerBase
    {
        private readonly ITaxCalculatorDomainInterface _taxCalculator;

        public TaxResultsController(ITaxCalculatorDomainInterface taxCalculator)
        {
            _taxCalculator = taxCalculator;
        }

        // GET: api/TaxResults
        [HttpGet("{amount}")]
        public async Task<ActionResult<TaxResultDto>> GetTaxResult(int amount)
        {
            return await Task.Run(() => _taxCalculator.TaxFor(amount));
        }
    }
}
