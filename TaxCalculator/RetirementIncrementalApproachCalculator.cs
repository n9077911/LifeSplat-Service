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
            var taxResult = new IncomeTaxCalculator().TaxFor(personStatus.Salary);
            var monthlyAfterTaxSalary = taxResult.Remainder / _monthly;
            var monthlySpending = personStatus.Spending / _monthly;

            var previousStep = new Step {Date = _now, Savings = personStatus.ExistingSavings};

            result.Steps.Add(previousStep);
            var calcdRetirementDate = false;

            for (var month = 1; month <= MonthsToDeath(personStatus.Dob, _now); month++)
            {
                var stepDate = previousStep.Date.AddMonths(1);
                var stepStatePensionAmount = calcdRetirementDate ? result.AnnualStatePension : _statePensionAmountCalculator.Calculate(personStatus, stepDate);
                var step = new Step { Date = stepDate };

                
                var savings = previousStep.Savings - monthlySpending;

                var growth = savings * _growthRate;
                step.Growth = growth;
                savings += growth;

                if (!calcdRetirementDate)
                {
                    savings += monthlyAfterTaxSalary;
                    step.AfterTaxSalary = monthlyAfterTaxSalary;
                }
                
                if (stepDate > statePensionDate)
                {
                    savings += stepStatePensionAmount / _monthly;
                    step.StatePension = stepStatePensionAmount / _monthly;
                }

                step.Savings = savings;
                result.Steps.Add(step);
                previousStep = step;

                if (!calcdRetirementDate &&
                    IsThatEnoughTillDeath(step.Savings, step.Date, minimumCash, personStatus, statePensionDate, stepStatePensionAmount))
                {
                    result.AnnualStatePension = Convert.ToInt32(stepStatePensionAmount);
                    result.RetirementDate = step.Date;
                    calcdRetirementDate = true;
                }
            }

            result.StateRetirementDate = statePensionDate;
            result.TimeToRetirement = new DateAmount(_now, result.RetirementDate);
            result.RetirementAge = AgeCalc.Age(personStatus.Dob, result.RetirementDate);
            result.StateRetirementAge = AgeCalc.Age(personStatus.Dob, result.StateRetirementDate);
            result.AfterTaxSalary = Convert.ToInt32(taxResult.Remainder);
            result.NationalInsuranceBill = Convert.ToInt32(taxResult.NationalInsurance);
            result.IncomeTaxBill = Convert.ToInt32(taxResult.IncomeTax);
            result.Spending = personStatus.Spending;
            
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
    }
}