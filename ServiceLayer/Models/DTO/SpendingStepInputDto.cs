using System;

namespace ServiceLayer.Models.DTO
{
    public class SpendingStepInputDto
    {
        public DateTime? Date { get; set; }
        public int Age { get; set; }
        public int Amount { get; set; }
    }
}