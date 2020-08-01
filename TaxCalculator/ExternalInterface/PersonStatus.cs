using System;

namespace TaxCalculator.ExternalInterface
{
    public class PersonStatus
    {
        public int ExistingSavings { get; set; }
        public int ExistingPension { get; set; }
        public int ExistingEmployeeContribution { get; set; }
        public int ExistingEmployerContribution { get; set; }
        public int Salary { get; set; }
        public int Spending { get; set; }
        public DateTime Dob { get; set; }
        public Sex Sex { get; set; }
    }
}