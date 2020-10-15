using System;
using System.Collections.Generic;

namespace Calculator.Input
{
    public class Person
    {
        private decimal _employeeContribution;
        private decimal _employerContribution;

        public int ExistingSavings { get; set; }
        public int ExistingPrivatePension { get; set; }
        public int? NiContributingYears { get; set; }
        public int Salary { get; set; }
        public DateTime Dob { get; set; }
        public Sex Sex { get; set; }
        public List<DateTime> Children { get; set; } = new List<DateTime>();
        public RentalPortfolio RentalPortfolio { get; set; } = new RentalPortfolio(new List<RentalInfo>());
        public EmergencyFundSpec EmergencyFundSpec { get; set; } = new EmergencyFundSpec("0"); //Cash needed for emergency fund.

        public decimal EmployeeContribution
        {
            get => _employeeContribution;
            set
            {
                if(value>1 || value < 0)
                    throw new Exception($"Invalid employee contribution of {value} given. Must be between 0 & 1 (i.e. 0 and 100%)");
                _employeeContribution = value;
            }
        }

        public decimal EmployerContribution
        {
            get => _employerContribution;
            set
            {
                if(value>1 || value < 0)
                    throw new Exception($"Invalid employer contribution of {value} given. Must be between 0 & 1 (i.e. 0 and 100%)");
                _employerContribution = value;
            }
        }
    }


    public class RentalInfo
    {
        public int GrossIncome { get; set; }
        public int Expenses { get; set; }
        public int MortgagePayments { get; set; }
        public int OutstandingMortgage { get; set; }
        public bool Repayment { get; set; }
        public bool RemainingTerm { get; set; }
        public decimal CurrentValue { get; set; }
    }
}