using System;
using System.Collections.Generic;

namespace Calculator.Output
{
    public interface IRetirementReport
    {
        DateAmount TimeToRetirement { get; }
        DateTime BankruptDate { get; }

        DateTime? TargetRetirementDate { get; }
        int? TargetRetirementAge { get; }
        
        DateTime FinancialIndependenceDate { get; }
        int FinancialIndependenceAge { get; }
        int SavingsAt100 { get; }
        
        IPersonReport PrimaryPerson { get; }
        List<IPersonReport> Persons { get; }
        List<SpendingStepReport> SpendingSteps { get; }
        
        int SavingsCombinedAtFinancialIndependenceAge { get; }
        int PrivatePensionCombinedAtFinancialIndependenceAge { get; }
        int? SavingsCombinedAtTargetRetirementAge { get; }
        int? PrivatePensionCombinedAtTargetRetirementAge { get; }
        
        decimal MonthlySpendingAt(DateTime date);
        void UpdateFinancialIndependenceDate(DateTime financialIndependenceDate);
        void ProcessResults(DateTime now);
        int CurrentSavingsRate();
        decimal RequiredEmergencyFund(DateTime date);
    }
}