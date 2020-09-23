using System.Collections.Generic;

namespace ServiceLayer.Models.DTO
{
    public class RetirementReportRequestDto
    {
        public int TargetRetirementAge { get; set; }
        public string TargetCashSavings { get; set; }
        public IEnumerable<PersonDto> Persons { get; set; }
        public IEnumerable<SpendingStepInputDto> SpendingSteps { get; set; }
    }
}