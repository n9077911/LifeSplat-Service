using System;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class RetirementIncrementalApproachCalculator : IRetirementCalculator
    {
        private readonly IPensionAgeCalc _pensionAgeCalc;
        private readonly IStatePensionAmountCalculator _statePensionAmountCalculator;
        private readonly int _monthly = 12;
        private readonly DateTime _now;
        private readonly int _estimatedDeath;
        private readonly decimal _growthRate;

        public RetirementIncrementalApproachCalculator(IDateProvider dateProvider,
            IAssumptions assumptions, IPensionAgeCalc pensionAgeCalc,
            IStatePensionAmountCalculator statePensionAmountCalculator)
        {
            _pensionAgeCalc = pensionAgeCalc;
            _statePensionAmountCalculator = statePensionAmountCalculator;
            _now = dateProvider.Now();
            _estimatedDeath = assumptions.EstimatedDeath;
            _growthRate = ConvertAnnualRateToMonthly(assumptions.GrowthRate);
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
            var monthlyAfterTaxSalary = MonthlyAfterTax(personStatus);
            var monthlySpending = personStatus.Spending / _monthly;

            var previousStep = new Step {Date = _now, Cash = 0};

            result.Steps.Add(previousStep);
            var calcdRetirementDate = false;

            for (var month = 1; month <= MonthsToDeath(personStatus.Dob, _now); month++)
            {
                var stepDate = previousStep.Date.AddMonths(1);
                var stepStatePensionAmount = calcdRetirementDate ? result.StatePensionAmount : _statePensionAmountCalculator.Calculate(personStatus, stepDate);
                var step = new Step { Date = stepDate };

                
                var amount = previousStep.Cash - monthlySpending;

                var growth = amount * _growthRate;
                step.Growth = growth;
                amount += growth;

                if (!calcdRetirementDate)
                {
                    amount += monthlyAfterTaxSalary;
                    step.AfterTaxSalary = monthlyAfterTaxSalary;
                }
                
                if (stepDate > statePensionDate)
                {
                    amount += stepStatePensionAmount / _monthly;
                    step.StatePension = stepStatePensionAmount / _monthly;
                }

                step.Cash = amount;
                result.Steps.Add(step);
                previousStep = step;

                if (!calcdRetirementDate &&
                    IsThatEnoughTillDeath(step.Cash, step.Date, minimumCash, personStatus, statePensionDate, stepStatePensionAmount))
                {
                    result.StatePensionAmount = stepStatePensionAmount;
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
            PersonStatus personStatus, DateTime statePensionDate, decimal statePensionAmount)
        {
            var monthsToDeath = MonthsToDeath(personStatus.Dob, now);
            var monthlySpending = personStatus.Spending / _monthly;
            var monthlyStatePension = statePensionAmount / _monthly;

            decimal runningCash = cash;
            for (int month = 1; month <= monthsToDeath; month++)
            {
                runningCash -= monthlySpending; 
                runningCash += runningCash*_growthRate;

                if(now.AddMonths(month) > statePensionDate)
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

        private decimal MonthlyAfterTax(PersonStatus personStatus)
        {
            var afterTaxSalary = new IncomeTaxCalculator().TaxFor(personStatus.Salary).Remainder;
            return afterTaxSalary / _monthly;
        }
    }
}