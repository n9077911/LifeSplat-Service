using System;
using System.Collections.Generic;

namespace TaxCalculator
{
    public interface IStepUpdater
    {
        void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator, DateTime statePensionDate);
        void UpdateGrowth();
        void UpdatePrivatePension(DateTime? givenRetirementDate);
        void UpdateSalary(decimal preTaxSalary);
        void UpdateSpending();
    }
}