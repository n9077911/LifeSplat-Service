using System;

namespace TaxCalculator.StatePensionCalculator
{
    public interface IPensionAgeCalc
    {
        DateTime StatePensionDate(DateTime dob, Sex sex);
        DateTime PrivatePensionDate(DateTime statePensionDate);
    }
}