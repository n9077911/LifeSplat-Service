using System;
using TaxCalcService.Models.DTO;

namespace TaxCalcService.Models
{
    public interface IRetirementDomainInterface
    {
        RetirementReportDto RetirementReportFor(int payeSalary, int spending, DateTime dob, bool female,
            int existingSavings, int existingPrivatePension, decimal employerContribution, decimal employeeContribution,
            int? targetRetirementAge);
    }
}