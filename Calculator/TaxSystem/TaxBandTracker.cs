using System;

namespace Calculator.TaxSystem
{
    /// <summary>
    /// Tracks how much of the given tax bands have been used up e.g. if someone earns 5k they have used 5k of the 12_500 tax free allowance
    /// </summary>
    internal class TaxBandTracker
    {
        private readonly decimal _basicRateBand;
        private readonly int _personalAllowanceWithdrawalLimit;

        private TaxBandTracker(decimal personalAllowance, decimal basicRateBand, decimal higherRateUpperBound, int personalAllowanceWithdrawalLimit)
        {
            PersonalAllowance = personalAllowance;
            _basicRateBand = basicRateBand;
            HigherRateUpperBound = higherRateUpperBound;
            _personalAllowanceWithdrawalLimit = personalAllowanceWithdrawalLimit;
        }
        
        public decimal PersonalAllowance { get; private set; }

        public decimal HigherBand => PersonalAllowance + _basicRateBand;
        
        public decimal HigherRateUpperBound { get; }
        
        public void UpdatePersonalAllowance(decimal payeSalary, decimal privatePension, decimal statePension, decimal rentalIncome)
        {
            var totalIncome = payeSalary + privatePension + statePension + rentalIncome;
            if (totalIncome > _personalAllowanceWithdrawalLimit)
            {
                PersonalAllowance = Math.Max(0m, PersonalAllowance - (totalIncome - _personalAllowanceWithdrawalLimit) / 2);
            }
        }

        public TaxBandTracker Subtract(decimal income)
        {
            return new TaxBandTracker(Math.Max(0, PersonalAllowance - income), 
                Math.Max(0, _basicRateBand - income), 
                Math.Max(0, HigherRateUpperBound - income), 
                _personalAllowanceWithdrawalLimit);
        }

        public static TaxBandTracker For(ITaxSystem taxSystem)
        {
            return new TaxBandTracker(taxSystem.PersonalAllowance, taxSystem.BasicRateBand,
                taxSystem.HigherRateUpperBound, taxSystem.PersonalAllowanceTaperPoint);
        }
    }
}