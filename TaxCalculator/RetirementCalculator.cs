using System;
using System.Collections.Generic;
using System.Globalization;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    //Produces a retirement report for a given person
    public class RetirementCalculator : IRetirementCalculator
    {
        private double _safeWithdrawalRate = 0.04;
        private int _monthly = 12;
        private DateTime _now;

        public RetirementCalculator(IDateProvider dateProvider)
        {
            _now = dateProvider.Now();
        }

        public IRetirementReport ReportFor(PersonStatus personStatus)
        {
            var result = new RetirementReport();

            result.TargetSavings = CalcTargetSavingsRequirement(personStatus);
            result.RetirementDate = CalcRetirementDate(personStatus, result);
            result.RetirementAge = AgeCalc.Age(personStatus.Dob, result.RetirementDate);

            result.TimeToRetirement = new DateAmount(_now, result.RetirementDate);

            return result;
        }

        private DateTime CalcRetirementDate(PersonStatus personStatus, RetirementReport retirementReport)
        {
            var afterTaxSalary = new IncomeTaxCalculator().TaxFor(personStatus.Salary).Remainder;
            var monthlySpareCash = (afterTaxSalary - personStatus.Spending) / _monthly;
            var months = (int) Math.Ceiling(retirementReport.TargetSavings / monthlySpareCash);

            var retirementReportRetirementDate = _now.AddMonths(months);
            return retirementReportRetirementDate;
        }

        private int CalcTargetSavingsRequirement(PersonStatus personStatus)
        {
            var target = personStatus.Spending / _safeWithdrawalRate;
            var ceiling = (int) Math.Ceiling(target);
            return ceiling;
        }
    }

    public class RetirementIncrementalApproachCalculator : IRetirementCalculator
    {
        private readonly IPensionAgeCalc _pensionAgeCalc;
        private readonly int _monthly = 12;
        private readonly DateTime _now;
        private readonly int _estimatedDeath;
        private readonly decimal _growthRate;
        private readonly decimal _statePensionAmount;

        public RetirementIncrementalApproachCalculator(IDateProvider dateProvider,
            SafeWithDrawlNoInflationAssumptions assumptions, IPensionAgeCalc pensionAgeCalc)
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
            var statePensionDate = _pensionAgeCalc.StatePensionDate(personStatus.Dob);

            var minimumCash = 0;
            var monthlySpareCash = MonthlySpareCash(personStatus);
            var monthlySpending = personStatus.Spending / _monthly;


            var previousStep = new Step {Date = _now, Cash = 0};
            result.Steps.Add(previousStep);
            var calcdRetirementDate = false;

            for (var month = 1; month <= MonthsToDeath(personStatus.Dob, _now); month++)
            {
                var stepDate = previousStep.Date.AddMonths(1);

                var amount = previousStep.Cash - monthlySpending;
                amount += amount * _growthRate;

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
    }

    public interface IAssumptions
    {
        int EstimatedDeath { get; }
        decimal GrowthRate { get; }
    }

    public class SafeWithDrawlNoInflationAssumptions : IAssumptions
    {
        public int EstimatedDeath => 100;
        public decimal GrowthRate => 0.04m;
        public decimal StatePension => 9110.4m;
    }

    public class Step
    {
        public DateTime Date { get; set; }
        public decimal Cash { get; set; }
    }
}