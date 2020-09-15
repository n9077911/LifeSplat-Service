using TaxCalcService.Models.DTO;

namespace TaxCalcService.Models
{
    public interface ITaxCalculatorDomainInterface
    {
        TaxResultDto TaxFor(int payeSalary);
    }
}