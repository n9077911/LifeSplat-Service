namespace TaxCalculator.ExternalInterface
{
    public interface ITaxResult
    {
        decimal IncomeTax { get; }
        decimal NationalInsurance { get; }
        decimal Total { get; }
    }
}