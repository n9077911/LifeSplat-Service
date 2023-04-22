using System.Collections.Generic;

namespace Calculator.TaxSystem
{
    public interface ITaxSystem
    {
        /// <summary>
        /// Lower earnings limit - defines how much someone must earn to receive a NI stamp towards their pension
        /// </summary>
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