using System;

namespace TaxCalculator.Input
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
}