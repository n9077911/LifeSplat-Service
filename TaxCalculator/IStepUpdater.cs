using System;

namespace TaxCalculator
{
    public interface IStepUpdater
    {
        void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator, DateTime statePensionDate);
        void UpdateGrowth();
        void UpdatePrivatePension(DateTime privatePensionDate);
        void UpdateSalary(decimal monthlyAfterTaxSalary);
        void UpdateSpending(decimal monthlySpending);
    }
}