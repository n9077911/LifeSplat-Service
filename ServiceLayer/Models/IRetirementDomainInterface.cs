using System;
using System.Collections.Generic;
using TaxCalcService.Models.DTO;
using TaxCalculator.ExternalInterface;

namespace TaxCalcService.Models
{
    public interface IRetirementDomainInterface
    {
        RetirementReportDto RetirementReportFor(int targetRetirementAge, IEnumerable<SpendingStepInputDto> spendingSteps, IEnumerable<PersonDto> persons);
    }
}