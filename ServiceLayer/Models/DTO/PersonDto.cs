using System;
using System.Collections.Generic;

namespace ServiceLayer.Models.DTO
{
    public class PersonDto
    {
        public int Salary { get; set; }
        public int Savings { get; set; }
        public int Pension { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal EmployeeContribution { get; set; }
        public int? NiContributingYears { get; set; }
        public string Dob { get; set; }
        public bool Female { get; set; }
        public List<RentalInfoDto> RentalInfo { get; set; }
        public List<DateTime> ChildrenDobs { get; set; }
    }

    public class RentalInfoDto
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