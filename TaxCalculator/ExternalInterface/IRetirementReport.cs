using System;
using System.Collections.Generic;

namespace TaxCalculator.ExternalInterface
{
    public interface IRetirementReport
    {
        int TargetSavings { get; }
        int RetirementAge { get; }
        int StateRetirementAge { get; }
        DateTime RetirementDate { get; }
        DateTime StateRetirementDate { get; }
        DateAmount TimeToRetirement { get; }
        List<Step> Steps { get; }
    }
}