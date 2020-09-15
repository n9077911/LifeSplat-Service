﻿using System.Collections.Generic;
using System.Linq;
using TaxCalcService.Models.DTO;
using TaxCalculator;
using TaxCalculator.ExternalInterface;
using TaxCalculator.Input;

namespace TaxCalcService.Models
{
    public class RetirementDomainInterface : IRetirementDomainInterface
    {
        private readonly IRetirementCalculator _retirementCalculator;

        public RetirementDomainInterface(IRetirementCalculator retirementCalculator)
        {
            _retirementCalculator = retirementCalculator;
        }

        public RetirementReportDto RetirementReportFor(int targetRetirementAge, IEnumerable<SpendingStepInputDto> spendingSteps, IEnumerable<PersonDto> persons)
        {
            var personsStatuses = persons.Select(PersonStatus);
            var spendingStepInputs = spendingSteps.Select(dto =>
            {
                var date = dto.Date ?? personsStatuses.First().Dob.AddYears(dto.Age.Value);
                return new SpendingStep(date, dto.Amount);
            });
            var retirementReport = _retirementCalculator.ReportForTargetAge(personsStatuses, spendingStepInputs, targetRetirementAge);

            var result = new RetirementReportDto(retirementReport);
            return result;
        }

        private static Person PersonStatus(PersonDto dto)
        {
            var personStatus = new Person
            {
                Dob = dto.Dob,
                Salary = dto.Salary,
                Sex = dto.Female ? Sex.Female : Sex.Male,
                ExistingSavings = dto.Savings,
                ExistingPrivatePension = dto.Pension,
                EmployerContribution = dto.EmployerContribution / 100m,
                EmployeeContribution = dto.EmployeeContribution / 100m,
                NiContributingYears = dto.NiContributingYears,
            };
            return personStatus;
        }
    }
}