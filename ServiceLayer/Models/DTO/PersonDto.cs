using System;

namespace TaxCalcService.Models.DTO
{
    public class PersonDto
    {
        public int Salary { get; set; }
        public int Savings { get; set; }
        public int Pension { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal EmployeeContribution { get; set; }
        public int? NiContributingYears { get; set; }
        public DateTime Dob { get; set; }
        public bool Female { get; set; }
    }
}