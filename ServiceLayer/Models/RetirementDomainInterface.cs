using System;
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
            var personList = persons.ToList();
            if (personList.Count == 2)
                emergencyFundSpec = emergencyFundSpec.SplitInTwo();

            var person = personList.Select(p => PersonStatus(p, emergencyFundSpec));
            var spendingStepInputs = spendingSteps.Select(dto => new SpendingStep(dto.Date ?? person.First().Dob.AddYears(dto.Age), dto.Amount));

            var retirementReport = await _retirementCalculator.ReportForTargetAgeAsync(person, spendingStepInputs, targetRetirementAge);

            return new RetirementReportDto(retirementReport);
        }

        private static Person PersonStatus(PersonDto dto, EmergencyFundSpec emergencyFundSpec)
        {
            var rentalInfos = dto.RentalInfo.Select(infoDto => new RentalInfo()
            {
                CurrentValue = infoDto.CurrentValue, Expenses = infoDto.Expenses, GrossIncome = infoDto.GrossIncome,
                Repayment = infoDto.Repayment, MortgagePayments = infoDto.MortgagePayments, OutstandingMortgage = infoDto.OutstandingMortgage, 
                RemainingTerm = infoDto.RemainingTerm
            });

            var personStatus = new Person
            {
                Dob = DateTime.Parse(dto.Dob),
                Salary = dto.Salary,
                Sex = dto.Female ? Sex.Female : Sex.Male,
                EmergencyFundSpec = emergencyFundSpec,
                ExistingSavings = dto.Savings,
                ExistingPrivatePension = dto.Pension,
                EmployerContribution = dto.EmployerContribution / 100m,
                EmployeeContribution = dto.EmployeeContribution / 100m,
                NiContributingYears = dto.NiContributingYears,
                RentalPortfolio = new RentalPortfolio(rentalInfos.ToList()),
                Children = dto.ChildrenDobs,
            };
            
            return personStatus;
        }
    }
}