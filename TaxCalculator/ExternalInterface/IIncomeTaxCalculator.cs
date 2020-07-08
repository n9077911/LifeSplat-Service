namespace TaxCalculator.ExternalInterface
{
    public interface IIncomeTaxCalculator
    {
        TaxResult TaxFor(int payeSalary);
    }
}