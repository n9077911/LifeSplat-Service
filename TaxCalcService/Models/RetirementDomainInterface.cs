using System;
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

        public RetirementReportDto RetirementReportFor(int payeSalary, int spending, DateTime dob, bool female)
        {
            var retirementReport = _retirementCalculator.ReportFor(new PersonStatus
            {
                Dob = dob, 
                Salary = payeSalary, 
                Spending = spending, 
                Sex = female ? Sex.Female : Sex.Male,
            });

            return new RetirementReportDto(retirementReport);
        }
    }
}