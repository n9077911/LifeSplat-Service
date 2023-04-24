using System;
using Calculator.Input;

namespace Calculator
{
    public class Assumptions : IAssumptions
    {
        public int EstimatedDeathAge { get; set; }
        public decimal AnnualGrowthRate { get; private init; }
        public decimal MonthlyGrowthRate => ConvertAnnualRateToMonthly(AnnualGrowthRate);
        public bool Take25 { get; set; }
        public int LifeTimeAllowance => 1_073_100;

        private Assumptions()
        {
        }

        public static Assumptions SafeWithdrawalNoInflationAssumptions(decimal annualGrowthRate = 0.04m)
        {
            return new Assumptions {EstimatedDeathAge = 100, AnnualGrowthRate = annualGrowthRate, Take25 = false};
        }
        
        public static Assumptions SafeWithdrawalNoInflationTake25Assumptions(decimal annualGrowthRate = 0.04m)
        {
            return new Assumptions {EstimatedDeathAge = 100, AnnualGrowthRate = annualGrowthRate, Take25 = true};
        }
        
        private static decimal ConvertAnnualRateToMonthly(decimal rate)
        {
            return (decimal) Math.Pow((double) (1 + rate), 1 / (double) 12) - 1;
        }
    }
}