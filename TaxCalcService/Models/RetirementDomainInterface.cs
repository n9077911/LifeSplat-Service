using System;
using System.Collections.Generic;
using System.Linq;
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

        public RetirementReportDto RetirementReportFor(int spending, int targetRetirementAge, IEnumerable<PersonDto> persons)
        {
            var personDto = persons.First();
            var retirementReport = _retirementCalculator.ReportForTargetAge(new PersonStatus
            {
                Dob = personDto.Dob, 
                Salary = personDto.Salary, 
                Spending = spending, 
                Sex = personDto.Female ? Sex.Female : Sex.Male,
                ExistingSavings = personDto.Savings,
                ExistingPrivatePension = personDto.Pension,
                EmployerContribution = personDto.EmployerContribution/100m,
                EmployeeContribution = personDto.EmployeeContribution/100m,
            }, targetRetirementAge);

            var result = new RetirementReportDto(retirementReport);
            return result;
        }
    }
}