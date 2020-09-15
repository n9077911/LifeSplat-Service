using ServiceLayer.Models.DTO;

namespace ServiceLayer.Models
{
    public interface ITaxCalculatorDomainInterface
    {
        TaxResultDto TaxFor(int payeSalary);
    }
}