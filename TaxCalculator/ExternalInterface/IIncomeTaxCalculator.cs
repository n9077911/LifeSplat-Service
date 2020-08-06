namespace TaxCalculator.ExternalInterface
{
    public interface IIncomeTaxCalculator
    {
        ITaxResult TaxFor(decimal payeSalary);
    }
}