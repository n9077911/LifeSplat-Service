using System;
// ReSharper disable All

namespace ServiceLayer.Models.DTO
{
    public class SpendingStepInputDto
    {
        public DateTime? Date { get; set; }
        public string Age { get; set; }
        public string Amount { get; set; }
    }
}