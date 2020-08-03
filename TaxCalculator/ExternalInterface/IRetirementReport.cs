using System;
using System.Collections.Generic;

namespace TaxCalculator.ExternalInterface
{
    public interface IRetirementReport
    {
        int TargetSavings { get; }
        int RetirementAge { get; }
        int StateRetirementAge { get; }
        int PrivateRetirementAge { get; }
        int AnnualStatePension { get; }
        int AfterTaxSalary { get; }
        int Spending { get; }
        int NationalInsuranceBill { get; }
        int IncomeTaxBill { get; }
        DateTime RetirementDate { get; }
        DateTime StateRetirementDate { get; }
        DateTime PrivateRetirementDate { get; }
        DateAmount TimeToRetirement { get; }
        List<Step> Steps { get; }
    }
}