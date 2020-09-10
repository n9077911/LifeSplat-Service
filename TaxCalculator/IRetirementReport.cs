using System;
using System.Collections.Generic;

namespace TaxCalculator
{
    public interface IRetirementReport
    {
        DateAmount TimeToRetirement { get; }
        DateTime BankruptDate { get; }

        DateTime? TargetRetirementDate { get; }
        int? TargetRetirementAge { get; }
        
        DateTime MinimumPossibleRetirementDate { get; }
        int MinimumPossibleRetirementAge { get; }
        int SavingsAtPrivatePensionAge { get; }
        int SavingsAtStatePensionAge { get; }
        int SavingsAtMinimumPossiblePensionAge { get; }
        int SavingsAt100 { get; }
        int PrivatePensionPotAtPrivatePensionAge { get; }
        int PrivatePensionPotAtStatePensionAge { get; }
        
        PersonReport PrimaryPerson { get; }
        List<PersonReport> Persons { get; }
        List<SpendingStep> SpendingSteps { get; }
        decimal MonthlySpendingAt(DateTime date);
    }
}