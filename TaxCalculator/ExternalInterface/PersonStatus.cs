using System;

namespace TaxCalculator.ExternalInterface
{
    public class PersonStatus
    {
        public int ExistingSavings { get; set; }
        public int ExistingPrivatePension { get; set; }
        public decimal EmployeeContribution { get; set; }
        public decimal EmployerContribution { get; set; }
        public int Salary { get; set; }
        public int Spending { get; set; }
        public DateTime Dob { get; set; }
        public Sex Sex { get; set; }
    }
}