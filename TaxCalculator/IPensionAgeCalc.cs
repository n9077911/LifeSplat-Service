using System;

namespace TaxCalculator
{
    public interface IPensionAgeCalc
    {
        DateTime StatePensionDate(DateTime dob, Sex sex);
        DateTime PrivatePensionDate(DateTime statePensionDate);
    }
}