using System;
using System.Collections.Generic;

namespace Calculator.Input
{
    public class Person
    {
        public Money ExistingSavings { get; set; } = Money.Create(0);
        public Money ExistingPrivatePension { get; set; } = Money.Create(0);
        public int? NiContributingYears { get; set; }
        public Money Salary { get; set; } = Money.Create(0);
        public DateTime Dob { get; set; }
        public Sex Sex { get; set; }
        public List<DateTime> Children { get; set; } = new List<DateTime>();
        public RentalPortfolio RentalPortfolio { get; set; } = new RentalPortfolio(new List<RentalInfo>());
        public EmergencyFundSpec EmergencyFundSpec { get; set; } = new EmergencyFundSpec("0"); //Cash needed for emergency fund.
        public PensionContribution EmployeeContribution { get; set; } = PensionContribution.Create(0m);
        public PensionContribution EmployerContribution { get; set; } = PensionContribution.Create(0m);
        public List<SalaryStep> SalaryStepInputs { get; set; } = new List<SalaryStep>();
    }


    public class RentalInfo
    {
        public Money GrossIncome { get; set; }
        public Money Expenses { get; set; }
        public Money MortgagePayments { get; set; }
        public Money OutstandingMortgage { get; set; }
        public bool Repayment { get; set; }
        public bool RemainingTerm { get; set; }
        public Money CurrentValue { get; set; }
    }
}