namespace Calculator.TaxSystem
{
    /// <summary>
    /// The output from an income tax calculation
    /// </summary>
    public interface ITaxResult
    {
        decimal IncomeTax { get; }
        decimal NationalInsurance { get; }
        decimal TotalTax { get; }
        decimal AfterTaxIncome { get; }
        decimal TotalTaxFor(IncomeType type);
        decimal AfterTaxIncomeFor(IncomeType type);
    }
}