using System.Collections.Generic;

namespace Calculator.TaxSystem
{
    public interface ITaxSystem
    {
        int LowerEarningsLimit { get; }
        IEnumerable<TaxBand> NationalInsuranceBands { get; }
        int PersonalAllowance { get; }
        int BasicRateBand { get; }
        int HigherRateUpperBound { get; }
        int PersonalAllowanceTaperPoint { get; }
        decimal AdditionalRate { get; }
        decimal HigherRate { get; }
        decimal BasicRate { get; }
    }
}