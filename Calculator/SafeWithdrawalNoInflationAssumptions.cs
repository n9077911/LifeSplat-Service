using System;
using Calculator.Input;

namespace Calculator
{
    public class Assumptions : IAssumptions
    {
        public int EstimatedDeathAge { get; set; }
        public decimal AnnualGrowthRate { get; set; }
        public decimal MonthlyGrowthRate => ConvertAnnualRateToMonthly(AnnualGrowthRate);
        public bool Take25 { get; set; }

        public static Assumptions SafeWithdrawalNoInflationAssumptions()
        {
            return new Assumptions {EstimatedDeathAge = 100, AnnualGrowthRate = 0.04m, Take25 = false};
        }
        
        public static Assumptions SafeWithdrawalNoInflationTake25Assumptions()
        {
            return new Assumptions {EstimatedDeathAge = 100, AnnualGrowthRate = 0.04m, Take25 = true};
        }
        
        private static decimal ConvertAnnualRateToMonthly(decimal rate)
        {
            return (decimal) Math.Pow((double) (1 + rate), 1 / (double) 12) - 1;
        }
    }
}