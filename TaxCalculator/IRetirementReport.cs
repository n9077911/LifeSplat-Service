using System;
using System.Collections.Generic;

namespace TaxCalculator
{
    public interface IRetirementReport
    {
        int Spending { get; }
        int MonthlySpending { get; }
        DateAmount TimeToRetirement { get; }
        DateTime BankruptDate { get; }
        DateTime MinimumPossibleRetirementDate { get; }
        int MinimumPossibleRetirementAge { get; }
        DateTime? TargetRetirementDate { get; }
        int? TargetRetirementAge { get; }
        int SavingsAtPrivatePensionAge { get; }
        int SavingsAtStatePensionAge { get; }
        int SavingsAtMinimumPossiblePensionAge { get; }
        int SavingsAt100 { get; }
        PersonReport PrimaryPerson { get; }
        int PrivatePensionPotAtPrivatePensionAge { get; }
        int PrivatePensionPotAtStatePensionAge { get; }
        List<PersonReport> Persons { get; }
    }
}