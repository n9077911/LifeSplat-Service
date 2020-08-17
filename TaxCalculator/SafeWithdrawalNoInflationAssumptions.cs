using System;

namespace TaxCalculator
{
    public class SafeWithdrawalNoInflationAssumptions : IAssumptions
    {
        public int EstimatedDeathAge => 100;
        public decimal AnnualGrowthRate => 0.04m;
        public decimal MonthlyGrowthRate => ConvertAnnualRateToMonthly(AnnualGrowthRate);
        
        private decimal ConvertAnnualRateToMonthly(decimal rate)
        {
            return (decimal) Math.Pow((double) (1 + rate), 1 / (double) 12) - 1;
        }
    }
}