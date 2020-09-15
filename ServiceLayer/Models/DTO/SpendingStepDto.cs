using System;

namespace TaxCalcService.Models.DTO
{
    public class SpendingStepDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Spending { get; set; }
    }
}