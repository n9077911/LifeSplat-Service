namespace TaxCalculator
{
    public class SafeWithdrawalNoInflationAssumptions : IAssumptions
    {
        public int EstimatedDeath => 100;
        public decimal GrowthRate => 0.04m;
    }
}