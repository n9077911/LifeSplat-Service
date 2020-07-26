using System;
using Microsoft.AspNetCore.Identity;
using TaxCalcService.Models.DTO;
using TaxCalculator;
using TaxCalculator.ExternalInterface;

namespace TaxCalcService.Models
{
    public class RetirementDomainInterface : IRetirementDomainInterface
    {
        private readonly IRetirementCalculator _retirementCalculator;

        public RetirementDomainInterface(IRetirementCalculator retirementCalculator)
        {
            _retirementCalculator = retirementCalculator;
        }

        public RetirementReportDto RetirementReportFor(int payeSalary, int spending, DateTime dob)
        {
            var retirementReport = _retirementCalculator.ReportFor(new PersonStatus {Dob = dob, Salary = payeSalary, Spending = spending, StatePensionAge = 68, Amount = 6_000});

            return new RetirementReportDto(retirementReport);
        }
    }
}