﻿using System;
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

        public RetirementReportDto RetirementReportFor(int targetRetirementAge, IEnumerable<PersonDto> persons)
        {
            var personsStatuses = persons.Select(PersonStatus);
            var retirementReport = _retirementCalculator.ReportForTargetAge(personsStatuses, targetRetirementAge);

            var result = new RetirementReportDto(retirementReport);
            return result;
        }

        private static PersonStatus PersonStatus(PersonDto dto)
        {
            var personStatus = new PersonStatus
            {
                Dob = dto.Dob,
                Salary = dto.Salary,
                Spending = dto.Spending,
                Sex = dto.Female ? Sex.Female : Sex.Male,
                ExistingSavings = dto.Savings,
                ExistingPrivatePension = dto.Pension,
                EmployerContribution = dto.EmployerContribution / 100m,
                EmployeeContribution = dto.EmployeeContribution / 100m,
            };
            return personStatus;
        }
    }
}