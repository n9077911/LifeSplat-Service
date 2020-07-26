using System;
using TaxCalculator.ExternalInterface;

namespace TaxCalcService.Models.DTO
{
    public class RetirementReportDto
    {
        public int RetirementAge { get; }
        public int StateRetirementAge { get; }
        public int TargetSavings { get; }
        public DateTime RetirementDate { get; }
        public DateTime StateRetirementDate { get; }
        public string TimeToRetirementDescription { get; }

        public RetirementReportDto(IRetirementReport retirementReport)
        {
            RetirementAge = retirementReport.RetirementAge;
            StateRetirementAge = retirementReport.StateRetirementAge;
            TargetSavings = retirementReport.TargetSavings;
            RetirementDate = retirementReport.RetirementDate;
            StateRetirementDate = retirementReport.StateRetirementDate;
            TimeToRetirementDescription = retirementReport.TimeToRetirement.ToString();
        }
    }
}