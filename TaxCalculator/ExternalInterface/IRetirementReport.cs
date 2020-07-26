using System;
using System.Collections.Generic;

namespace TaxCalculator.ExternalInterface
{
    public interface IRetirementReport
    {
        int TargetSavings { get; }
        DateTime RetirementDate { get; }
        int RetirementAge { get; }
        DateAmount TimeToRetirement { get; }
        List<Step> Steps { get; }
    }
}