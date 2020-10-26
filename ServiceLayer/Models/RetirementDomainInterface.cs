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

        public async Task<RetirementReportDto> RetirementReportForAsync(RetirementReportRequestDto requestDto)
        {
            var emergencyFundSpec = new EmergencyFundSpec(requestDto.EmergencyFund);
            var personList = requestDto.Persons.ToList();
            if (personList.Count == 2)
                emergencyFundSpec = emergencyFundSpec.SplitInTwo();

            var person = personList.Select(p => PersonStatus(p, emergencyFundSpec));
            var spendingStepInputs = new List<SpendingStep> {new SpendingStep(DateTime.Now.Date, Money.Create(requestDto.Spending))};
            spendingStepInputs.AddRange(requestDto.SpendingSteps.Select(dto => new SpendingStep(dto.Date ?? person.First().Dob.AddYears(Convert.ToInt32(dto.Age)), Money.Create(dto.Amount))));

            var retirementReport = await _retirementCalculator.ReportForTargetAgeAsync(person, spendingStepInputs, Age.Create(requestDto.TargetRetirementAge));

            return new RetirementReportDto(retirementReport);
        }

        private static Person PersonStatus(PersonDto dto, EmergencyFundSpec emergencyFundSpec)
        {
            var rentalInfos = dto.Rental == null ? new List<RentalInfo>() : dto.Rental.Select(infoDto => new RentalInfo()
            {
                CurrentValue = Money.Create(infoDto.CurrentValue),
                Expenses = Money.Create(infoDto.Expenses),
                GrossIncome = Money.Create(infoDto.GrossIncome),
                Repayment = infoDto.Repayment,
                MortgagePayments = Money.Create(infoDto.MortgagePayments),
                OutstandingMortgage = Money.Create(infoDto.OutstandingMortgage), 
                RemainingTerm = infoDto.RemainingTerm
            });

            var person = new Person
            {
                Dob = DateTime.Parse(dto.Dob),
                Salary = Money.Create(dto.Salary),
                Sex = dto.Female ? Sex.Female : Sex.Male,
                EmergencyFundSpec = emergencyFundSpec,
                ExistingSavings = Money.Create(dto.Savings),
                ExistingPrivatePension = Money.Create(dto.Pension),
                EmployerContribution = PensionContribution.Create(dto.EmployerContribution),
                EmployeeContribution = PensionContribution.Create(dto.EmployeeContribution),
                NiContributingYears = string.IsNullOrWhiteSpace(dto.NiContributingYears) ? (int?)null : Convert.ToInt32(dto.NiContributingYears),
                RentalPortfolio = new RentalPortfolio(rentalInfos.ToList()), 
                Children = dto.Children ?? new List<DateTime>(), 
            }; 
            
            return person;  
        }
    } 

}