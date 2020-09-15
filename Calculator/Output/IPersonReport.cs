using System;
using System.Collections.Generic;
using Calculator.Input;

namespace Calculator.Output
{
    public interface IPersonReport
    {
        Person Person { get; }
        List<StepsReport> StepReports { get; }
        StepsReport CalcMinimumSteps { get; }
        StepsReport TargetSteps { get; }
        StepsReport PrimarySteps { get; }
        decimal MonthlySalaryAfterDeductions { get; }
        int NationalInsuranceBill { get; }
        int IncomeTaxBill { get; }
        int AfterTaxSalary { get; }
        DateTime StatePensionDate { get; set; }
        DateTime PrivatePensionDate { get; set; }
        int BankruptAge { get; set; }
        int StatePensionAge { get; set; }
        int PrivatePensionAge { get; set; }
        int AnnualStatePension { get; set; }
        int NiContributingYears { get; set; }
        int PrivatePensionPotCombinedAtPrivatePensionAge { get; set; }
        int PrivatePensionPotCombinedAtStatePensionAge { get; set; }
        int PrivatePensionSafeWithdrawal { get; set; }
        DateTime MinimumPossibleRetirementDate { get; set; }
        int MinimumPossibleRetirementAge { get; }
        int SavingsAtMinimumPossiblePensionAge { get; set; }
        int SavingsCombinedAtPrivatePensionAge { get; set; }
        int SavingsCombinedAtStatePensionAge { get; set; }
        int PrivatePensionPotAtPrivatePensionAge { get; set; }
    }
}