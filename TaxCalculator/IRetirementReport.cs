using System;
using System.Collections.Generic;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public interface IRetirementReport
    {
        decimal MonthlySpending { get; }
        int Spending { get; set; }
        DateAmount TimeToRetirement { get; set; }
        DateTime BankruptDate { get; set; }
        List<Step> Steps { get; }
        Dictionary<PersonStatus, List<Step>> StepsDict { get; }
        Dictionary<PersonStatus, PersonReport> PersonReports { get; }
        DateTime MinimumPossibleRetirementDate { get; set; }
        int MinimumPossibleRetirementAge { get; set; }
        DateTime? TargetRetirementDate { get; set; }
        int? TargetRetirementAge { get; set; }
        int SavingsAtPrivatePensionAge { get; set; }
        int SavingsAtStatePensionAge { get; set; }
        int SavingsAtMinimumPossiblePensionAge { get; set; }
        int SavingsAt100 { get; set; }
        PersonReport PrimaryPerson { get; }
    }
}