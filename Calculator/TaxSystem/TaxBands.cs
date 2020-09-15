using System;

namespace TaxCalculator.TaxSystem
{
    /// <summary>
    /// Tracks how much of the given tax bands have been used up e.g. if someone earns 5k they have used 5k of the 12_500 tax free allowance
    /// </summary>
    internal class TaxBands
    {
        private readonly decimal _lowerBandLimit;
        private readonly int _personalAllowanceWithdrawalLimit;

        public decimal PersonalAllowance { get; private set; }
        public decimal HigherBand => PersonalAllowance + _lowerBandLimit;
        public decimal ExtraHighBand { get; }

        private TaxBands(decimal personalAllowance, decimal lowerBandLimit, decimal extraHighBand, int personalAllowanceWithdrawalLimit)
        {
            PersonalAllowance = personalAllowance;
            _lowerBandLimit = lowerBandLimit;
            ExtraHighBand = extraHighBand;
            _personalAllowanceWithdrawalLimit = personalAllowanceWithdrawalLimit;
        }

        public static TaxBands InitialEngland2020()
        {
            return new TaxBands(12_509, 37_500, 150_000, 100_000);
        }

        public void UpdatePersonalAllowance(decimal payeSalary, decimal privatePension, decimal statePension)
        {
            var totalIncome = payeSalary + privatePension + statePension;
            if (totalIncome > _personalAllowanceWithdrawalLimit)
            {
                PersonalAllowance = Math.Max(0m, PersonalAllowance - (totalIncome - _personalAllowanceWithdrawalLimit) / 2);
            }
        }

        public TaxBands Subtract(decimal income)
        {
            return new TaxBands(Math.Max(0, PersonalAllowance - income), Math.Max(0, _lowerBandLimit - income), Math.Max(0, ExtraHighBand - income), _personalAllowanceWithdrawalLimit);
        }
    }
}