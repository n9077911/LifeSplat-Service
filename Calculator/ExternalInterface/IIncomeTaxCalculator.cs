using Calculator.TaxSystem;

namespace Calculator.ExternalInterface
{
    public interface IIncomeTaxCalculator
    {
        /// <summary>
        /// Generates a report detailing the tax a person pays
        /// </summary>
        ITaxResult TaxFor(decimal payeSalary, decimal privatePension = 0, decimal statePension = 0);
    }
}