using System;
using TaxCalculator.ExternalInterface;

namespace TaxCalcService.Models.DTO
{
    public class RetirementReportDto
    {
        public int Age { get; }
        public int TargetSavings { get; }
        public DateTime RetirementDate { get; }
        public string TimeToRetirementDescription { get; }

        public RetirementReportDto(IRetirementReport retirementReport)
        {
            Age = retirementReport.RetirementAge;
            TargetSavings = retirementReport.TargetSavings;
            RetirementDate = retirementReport.RetirementDate;
            TimeToRetirementDescription = retirementReport.TimeToRetirement.ToString();
        }
    }
}