using System;

namespace TaxCalculator.ExternalInterface
{
    public interface IRetirementReport
    {
        int TargetSavings { get; }
        DateTime RetirementDate { get; set; }
        int RetirementAge { get; set; }
        int YearsToRetirement { get; set; }
        DateAmount TimeToRetirement { get; }
    }
}