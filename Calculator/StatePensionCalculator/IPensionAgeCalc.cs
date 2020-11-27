using System;

namespace Calculator.StatePensionCalculator
{
    public interface IPensionAgeCalc
    {
        DateTime StatePensionDate(DateTime dob, Sex sex);
        DateTime PrivatePensionDate(DateTime statePensionDate, DateTime dob);
    }
}