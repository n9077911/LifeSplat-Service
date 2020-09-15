using System.Collections.Generic;

namespace ServiceLayer.Models.DTO
{
    public class TaxResultDto
    {
        public List<TaxResultItemDto> TaxResultItems { get; set; } = new List<TaxResultItemDto>();
    }
}