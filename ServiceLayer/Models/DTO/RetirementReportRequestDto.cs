using System.Collections.Generic;

namespace ServiceLayer.Models.DTO
{
    public class RetirementReportRequestDto
    {
        public string TargetRetirementAge { get; set; }
        public string EmergencyFund { get; set; }
        public IEnumerable<PersonDto> Persons { get; set; }
        public IEnumerable<SpendingStepInputDto> SpendingSteps { get; set; }
        public string Spending { get; set; }
        public string AnnualGrowthRate { get; set; }
    }
}