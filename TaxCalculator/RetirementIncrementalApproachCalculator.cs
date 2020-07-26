using System;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class RetirementIncrementalApproachCalculator : IRetirementCalculator
    {
        private readonly IPensionAgeCalc _pensionAgeCalc;
        private readonly int _monthly = 12;
        private readonly DateTime _now;
        private readonly int _estimatedDeath;
        private readonly decimal _growthRate;
        private readonly decimal _statePensionAmount;

        public RetirementIncrementalApproachCalculator(IDateProvider dateProvider,
            IAssumptions assumptions, IPensionAgeCalc pensionAgeCalc)
        {
            _pensionAgeCalc = pensionAgeCalc;
            _now = dateProvider.Now();
            _estimatedDeath = assumptions.EstimatedDeath;
            _growthRate = ConvertAnnualRateToMonthly(assumptions.GrowthRate);
            _statePensionAmount = assumptions.StatePension;
        }

        private decimal ConvertAnnualRateToMonthly(decimal rate)
        {
            return (decimal)Math.Pow((double)(1+rate),((double)1/_monthly))-1;
        }

        public IRetirementReport ReportFor(PersonStatus personStatus)
        {
            var result = new RetirementReport();
            var statePensionDate = _pensionAgeCalc.StatePensionDate(personStatus.Dob, personStatus.Sex);

            var minimumCash = 0;
            var monthlySpareCash = MonthlySpareCash(personStatus);
            monthlySpareCash = MonthlyAfterTax(personStatus);
            var monthlySpending = personStatus.Spending / _monthly;


            var previousStep = new Step {Date = _now, Cash = 0};
            result.Steps.Add(previousStep);
            var calcdRetirementDate = false;

            for (var month = 1; month <= MonthsToDeath(personStatus.Dob, _now); month++)
            {
                var stepDate = previousStep.Date.AddMonths(1);

                var amount = previousStep.Cash - monthlySpending;

                amount += previousStep.Cash * _growthRate;

                if (!calcdRetirementDate)
                    amount += monthlySpareCash;
                if(stepDate > statePensionDate)
                    amount += _statePensionAmount / _monthly;
                

                
                var step = new Step { Date = stepDate, Cash = amount };
                result.Steps.Add(step);
                previousStep = step;

                if (!calcdRetirementDate &&
                    IsThatEnoughTillDeath(step.Cash, step.Date, minimumCash, personStatus, statePensionDate))
                {
                    result.RetirementDate = step.Date;
                    calcdRetirementDate = true;
                }
            }

            result.StateRetirementDate = statePensionDate;
            result.TimeToRetirement = new DateAmount(_now, result.RetirementDate);
            result.RetirementAge = AgeCalc.Age(personStatus.Dob, result.RetirementDate);
            result.StateRetirementAge = AgeCalc.Age(personStatus.Dob, result.StateRetirementDate);
          
            return result;
        }

        private bool IsThatEnoughTillDeath(decimal cash, DateTime now, int minimumCash,
            PersonStatus personStatus, DateTime stateRetirementDate)
        {
            var monthsToDeath = MonthsToDeath(personStatus.Dob, now);
            var monthlySpending = personStatus.Spending / _monthly;
            var monthlyStatePension = _statePensionAmount / _monthly;

            decimal runningCash = cash;
            for (int month = 1; month <= monthsToDeath; month++)
            {
                runningCash = runningCash + runningCash*_growthRate - monthlySpending;

                if (stateRetirementDate < now.AddMonths(month))
                    runningCash += monthlyStatePension;

                if (runningCash < minimumCash)
                    return false;
            }

            return true;
        }

        private int MonthsToDeath(DateTime dob, DateTime now)
        {
            var dateAmount = new DateAmount(now, dob.AddYears(_estimatedDeath));
            return dateAmount.TotalMonths();
        }

        private decimal MonthlySpareCash(PersonStatus personStatus)
        {
            var afterTaxSalary = new IncomeTaxCalculator().TaxFor(personStatus.Salary).Remainder;
            return (afterTaxSalary - personStatus.Spending) / _monthly;
        }
        
        private decimal MonthlyAfterTax(PersonStatus personStatus)
        {
            var afterTaxSalary = new IncomeTaxCalculator().TaxFor(personStatus.Salary).Remainder;
            return afterTaxSalary / _monthly;
        }
    }
}