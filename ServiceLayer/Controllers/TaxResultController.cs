using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Models;
using ServiceLayer.Models.DTO;

namespace ServiceLayer.Controllers
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
        public TaxResultDto GetTaxResult(int amount)
        {
            return _taxCalculator.TaxFor(amount);
        }
    }
}
