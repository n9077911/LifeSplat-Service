using TaxCalculator.TaxSystem;

namespace TaxCalculator.ExternalInterface
{
    public interface ITaxResult
    {
        decimal IncomeTax { get; }
        decimal NationalInsurance { get; }
        decimal Total { get; }
        decimal Remainder { get; }
        decimal IncomeTaxFor(IncomeType type);
        decimal RemainderFor(IncomeType type);
    }
}