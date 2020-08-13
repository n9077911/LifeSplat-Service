using System.Collections.Generic;

namespace TaxCalcService.Models.DTO
{
    public class RetirementReportRequestDto
    {
        public int Spending { get; set; }
        public int TargetRetirementAge { get; set; }
        public IEnumerable<PersonDto> Persons { get; set; }
    }
}