using System;
using System.Collections.Generic;
using TaxCalcService.Models.DTO;

namespace TaxCalcService.Models
{
    public interface IRetirementDomainInterface
    {
        RetirementReportDto RetirementReportFor(int targetRetirementAge, IEnumerable<PersonDto> persons);
    }
}