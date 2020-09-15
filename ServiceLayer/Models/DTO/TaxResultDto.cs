using System.Collections.Generic;

namespace TaxCalcService.Models.DTO
{
    public class TaxResultDto
    {
        public List<TaxResultItemDto> TaxResultItems { get; set; } = new List<TaxResultItemDto>();
    }
}