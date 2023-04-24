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
            var spendingStepInputs = new List<SpendingStep> {new(DateTime.Now.Date, Money.Create(requestDto.Spending))};
            spendingStepInputs.AddRange(requestDto.SpendingSteps.Select(dto => new SpendingStep(dto.Date ?? person.First().Dob.AddYears(Convert.ToInt32(dto.Age)), Money.Create(dto.Amount))));

            IAssumptions assumptions = Assumptions.SafeWithdrawalNoInflationTake25Assumptions();
            if (!string.IsNullOrWhiteSpace(requestDto.AnnualGrowthRate))
            {
                var annualGrowthRate = decimal.Parse(requestDto.AnnualGrowthRate);
                assumptions = Assumptions.SafeWithdrawalNoInflationTake25Assumptions(annualGrowthRate);
            }

            var retirementReport = await _retirementCalculator.ReportForTargetAgeAsync(person, spendingStepInputs, Age.Create(requestDto.TargetRetirementAge), assumptions);

            return new RetirementReportDto(retirementReport);
        }

        private static Person PersonStatus(PersonDto dto, EmergencyFundSpec emergencyFundSpec)
        {
            var rentalInfos = dto.Rental == null ? new List<RentalInfo>() : dto.Rental.Select(infoDto => new RentalInfo()
            {
                CurrentValue = Money.Create(infoDto.CurrentValue),
                Expenses = Money.Create(infoDto.Expenses),
                GrossIncome = Money.Create(infoDto.GrossRent),
                Repayment = infoDto.Repayment,
                MortgagePayments = Money.Create(infoDto.MortgageCosts),
                OutstandingMortgage = Money.Create(infoDto.OutstandingMortgage), 
                RemainingTerm = infoDto.RemainingTerm
            });

            var dob = DateTime.Parse(dto.Dob);
            
            var salaryStepInputs = new List<SalaryStep>();
            var salarySteps = dto.SalarySteps.Where(step => !string.IsNullOrWhiteSpace(step.Age) && !string.IsNullOrWhiteSpace(step.Amount))
                .Select(step => new SalaryStep(step.Date ?? dob.AddYears(Convert.ToInt32(step.Age)), Money.Create(step.Amount)));
            salaryStepInputs.AddRange(salarySteps);
            
            var person = new Person
            {
                Dob = dob,
                Salary = Money.Create(dto.Salary),
                SalaryStepInputs = salaryStepInputs, 
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