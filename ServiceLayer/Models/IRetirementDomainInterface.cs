using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLayer.Models.DTO;

namespace ServiceLayer.Models
{
    public interface IRetirementDomainInterface
    {
        Task<RetirementReportDto> RetirementReportForAsync(RetirementReportRequestDto requestDto);
    }
}