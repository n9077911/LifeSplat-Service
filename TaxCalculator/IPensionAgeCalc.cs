using System;

namespace TaxCalculator
{
    public interface IPensionAgeCalc
    {
        DateTime StatePensionDate(DateTime personStatusDob);
        DateTime PrivatePensionDate();
    }
}