using System;

namespace TaxCalcService.Models.DTO
{
    public class PersonDto
    {
        public int Salary { get; set; }
        public int Savings { get; set; }
        public int Pension { get; set; }
        public int EmployerContribution { get; set; }
        public int EmployeeContribution { get; set; }
        public int? NiContributingYears { get; set; }
        public DateTime Dob { get; set; }
        public bool Female { get; set; }
        public int Spending { get; set; }
    }
}