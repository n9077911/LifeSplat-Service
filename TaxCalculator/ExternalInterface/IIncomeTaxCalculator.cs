namespace TaxCalculator.ExternalInterface
{
    public interface IIncomeTaxCalculator
    {
        ITaxResult TaxFor(int payeSalary);
    }
}