using System;
using System.Collections.Generic;

namespace ServiceLayer.Models.DTO
{
    public class PersonDto
    {
        public string Salary { get; set; }
        public string Savings { get; set; }
        public string Pension { get; set; }
        public string EmployerContribution { get; set; }
        public string EmployeeContribution { get; set; }
        public string NiContributingYears { get; set; }
        public string Dob { get; set; }
        public bool Female { get; set; }
        public List<RentalInfoDto> Rental { get; set; }
        public List<DateTime> Children { get; set; }
    }

    public class RentalInfoDto
    {
        public string GrossIncome { get; set; }
        public string Expenses { get; set; }
        public string MortgagePayments { get; set; }
        public string OutstandingMortgage { get; set; }
        public bool Repayment { get; set; }
        public bool RemainingTerm { get; set; }
        public string CurrentValue { get; set; }
    }
}