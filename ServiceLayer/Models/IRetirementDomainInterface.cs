using System;
using System.Collections.Generic;
using Calculator.ExternalInterface;
using ServiceLayer.Models.DTO;

namespace ServiceLayer.Models
{
    public interface IRetirementDomainInterface
    {
        RetirementReportDto RetirementReportFor(int targetRetirementAge, string targetCashSavings, IEnumerable<SpendingStepInputDto> spendingSteps, IEnumerable<PersonDto> persons);
    }
}