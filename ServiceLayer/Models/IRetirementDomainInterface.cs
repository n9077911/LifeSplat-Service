using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLayer.Models.DTO;

namespace ServiceLayer.Models
{
    public interface IRetirementDomainInterface
    {
        Task<RetirementReportDto> RetirementReportForAsync(int? targetRetirementAge, string emergencyFund, IEnumerable<SpendingStepInputDto> spendingSteps, IEnumerable<PersonDto> persons);
    }
}