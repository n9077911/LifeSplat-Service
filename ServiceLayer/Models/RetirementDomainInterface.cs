using System.Collections.Generic;
using System.Linq;
using Calculator;
using Calculator.ExternalInterface;
using Calculator.Input;
using System.Threading.Tasks;
using ServiceLayer.Models.DTO;

namespace ServiceLayer.Models
{
    /// <summary>
    /// RetirementCalculator gateway. Knows how to translate given DTOs from the client into a format required by RetirementCalculator.
    /// </summary>
    public class RetirementDomainInterface : IRetirementDomainInterface
    {
        private readonly IRetirementCalculator _retirementCalculator;

        public RetirementDomainInterface(IRetirementCalculator retirementCalculator)
        {
            _retirementCalculator = retirementCalculator;
        }

        public async Task<RetirementReportDto> RetirementReportForAsync(int? targetRetirementAge, string emergencyFund, IEnumerable<SpendingStepInputDto> spendingSteps, IEnumerable<PersonDto> persons)
        {
            var emergencyFundSpec = new EmergencyFundSpec(emergencyFund);
            if (persons.Count() == 2)
                emergencyFundSpec = emergencyFundSpec.SplitInTwo();
            
            var personsStatuses = persons.Select(p => PersonStatus(p, emergencyFundSpec));
            var spendingStepInputs = spendingSteps.Select(dto =>
            {
                var date = dto.Date ?? personsStatuses.First().Dob.AddYears(dto.Age);
                return new SpendingStep(date, dto.Amount);
            });
            var retirementReport = await _retirementCalculator.ReportForTargetAgeAsync(personsStatuses, spendingStepInputs, targetRetirementAge);

            var result = new RetirementReportDto(retirementReport);
            return result;
        }

        private static Person PersonStatus(PersonDto dto, EmergencyFundSpec emergencyFundSpec)
        {
            var personStatus = new Person
            {
                Dob = dto.Dob,
                Salary = dto.Salary,
                Sex = dto.Female ? Sex.Female : Sex.Male,
                EmergencyFundSpec = emergencyFundSpec,
                ExistingSavings = dto.Savings,
                ExistingPrivatePension = dto.Pension,
                EmployerContribution = dto.EmployerContribution / 100m,
                EmployeeContribution = dto.EmployeeContribution / 100m,
                NiContributingYears = dto.NiContributingYears,
            };
            return personStatus;
        }
    }
}