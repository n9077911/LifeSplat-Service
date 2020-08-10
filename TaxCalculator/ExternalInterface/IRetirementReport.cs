using System;
using System.Collections.Generic;

namespace TaxCalculator.ExternalInterface
{
    public interface IRetirementReport
    {
        int MinimumPossibleRetirementAge { get; }
        int StateRetirementAge { get; }
        int PrivateRetirementAge { get; }
        int AnnualStatePension { get; }
        int QualifyingStatePensionYears { get; }
        int PrivatePensionPot { get; }
        int PrivatePensionSafeWithdrawal { get; }
        int AfterTaxSalary { get; }
        int Spending { get; }
        int NationalInsuranceBill { get; }
        int IncomeTaxBill { get; }
        DateTime MinimumPossibleRetirementDate { get; }
        DateTime? TargetRetirementDate { get; }
        int? TargetRetirementAge { get; }
        DateTime StateRetirementDate { get; }
        DateTime PrivateRetirementDate { get; }
        int SavingsAtPrivatePensionAge { get; }
        int SavingsAtStatePensionAge { get; }
        int SavingsAtMinimumPossiblePensionAge { get; }
        int SavingsAt100 { get; }
        DateAmount TimeToRetirement { get; }
        DateTime BankruptDate { get; }
        List<Step> Steps { get; }
    }
}