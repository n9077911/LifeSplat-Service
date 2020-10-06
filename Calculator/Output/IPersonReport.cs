using System;
using System.Collections.Generic;
using Calculator.Input;

namespace Calculator.Output
{
    public interface IPersonReport
    {
        Person Person { get; }
        StepsReport StepReport { get; }
        decimal MonthlySalaryAfterDeductions { get; }
        int NationalInsuranceBill { get; }
        int IncomeTaxBill { get; }
        int RentalTaxBill { get; }
        int TakeHomeSalary { get; }
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
        void UpdateMinimumPossibleRetirementDate(in DateTime minimumPossibleRetirementDate);
        bool Take25WhenRetired(in bool calcdMinimum, in DateTime now, DateTime? givenRetirementDate);
    }
}