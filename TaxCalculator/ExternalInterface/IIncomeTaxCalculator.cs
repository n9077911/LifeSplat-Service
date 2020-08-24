namespace TaxCalculator.ExternalInterface
{
    public interface IIncomeTaxCalculator
    {
        ITaxResult TaxFor(decimal payeSalary, decimal privatePension = 0, decimal statePension = 0);
    }
}