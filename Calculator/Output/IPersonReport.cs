using System;
using System.Collections.Generic;
using Calculator.Input;

namespace Calculator.Output
{
    public interface IPersonReport
    {
        Person Person { get; }
        StepsReport StepReport { get; }
        int NationalInsuranceBill { get; }
        int IncomeTaxBill { get; }
        int RentalTaxBill { get; }
        int TakeHomeSalary { get; }
        int TakeHomeRentalIncome { get; }
        int PensionContributions { get; }
        
        DateTime StatePensionDate { get; set; }
        DateTime PrivatePensionDate { get; }
        DateTime FinancialIndependenceDate { get; set; }
        
        int BankruptAge { get; set; }
        int StatePensionAge { get; }
        int PrivatePensionAge { get; }
        int? TargetRetirementAge { get; }
        int PrivatePensionCrystallisationAge { get; }
        int FinancialIndependenceAge { get; }
        
        int AnnualStatePension { get; set; }
        int NiContributingYears { get; set; }
        int PrivatePensionSafeWithdrawal { get; set; }
        
        int PrivatePensionPotAtFinancialIndependenceAge { get; set; }
        int SavingsAtFinancialIndependenceAge { get; set; }
        int? PrivatePensionPotAtTargetRetirementAge { get; set; }
        int? SavingsAtTargetRetirementAge { get; set; }
        
        DateTime PrivatePensionPotCrystallisationDate { get; }

        int PrivatePensionPotAtCrystallisationAge { get; }
        int PrivatePensionPotBeforeCrystallisation { get; }
        int Take25LumpSum { get; }
        
        bool Retired(in bool calcdMinimum, in DateTime now, DateTime? givenRetirementDate);
        void UpdateFinancialIndependenceDate(in DateTime minimumPossibleRetirementDate);
        void CrystallisePension();
        void UpdateWithConclusions(IAssumptions assumptions, DateTime bankruptDate);
        IPersonReport CopyFormCalcMinimumMode();
        decimal MonthlySalaryAfterDeductionsAt(DateTime date);
    }
}