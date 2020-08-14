using System;
using System.Collections.Generic;
using System.Linq;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class RetirementIncrementalApproachCalculator : IRetirementCalculator
    {
        private readonly IPensionAgeCalc _pensionAgeCalc;
        private readonly IStatePensionAmountCalculator _statePensionAmountCalculator;
        private readonly decimal _monthly = 12;
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
            return (decimal) Math.Pow((double) (1 + rate), 1 / (double) _monthly) - 1;
        }

        public IRetirementReport ReportForTargetAge(PersonStatus personStatus, int? retirementAge = null)
        {
            return ReportForTargetAge(new[] {personStatus}, retirementAge);
        }

        public IRetirementReport ReportForTargetAge(IEnumerable<PersonStatus> personStatus, int? retirementAge = null)
        {
            return ReportFor(personStatus, retirementAge.HasValue && retirementAge.Value != 0 ? personStatus.First().Dob.AddYears(retirementAge.Value) : (DateTime?) null);
        }

        public IRetirementReport ReportFor(PersonStatus personStatus, DateTime? givenRetirementDate = null)
        {
            return ReportFor(new[] {personStatus}, givenRetirementDate);
        }

        public IRetirementReport ReportFor(IEnumerable<PersonStatus> personStatuses, DateTime? givenRetirementDate = null)
        {
            var personStatus = personStatuses.First();
            var result = new RetirementReport(_pensionAgeCalc, personStatus);
            var statePensionDate = _pensionAgeCalc.StatePensionDate(personStatus.Dob, personStatus.Sex);
            var privatePensionDate = _pensionAgeCalc.PrivatePensionDate(statePensionDate);

            var targetDateGiven = givenRetirementDate.HasValue;

            var emergencyFund = 0;


            var stepDescriptions = new[] {new StepDescription(StepType.CalcMinimum), new StepDescription(StepType.GivenDate)};
            foreach (var stepDescription in stepDescriptions)
            {
                stepDescription.SetInitialStep(new Step
                {
                    Date = _now, Savings = personStatus.ExistingSavings,
                    PrivatePensionAmount = personStatus.ExistingPrivatePension, PrivatePensionGrowth = personStatus.ExistingPrivatePension * _growthRate
                });
            }
            result.Steps.Add(targetDateGiven ? stepDescriptions[1].CurrentStep : stepDescriptions[0].CurrentStep );

            var calcdMinimum = false;

            for (var month = 1; month <= MonthsToDeath(personStatus.Dob, _now); month++)
            {
                foreach (var stepDescription in stepDescriptions)
                {
                    stepDescription.NewStep(personStatus, calcdMinimum, givenRetirementDate);
                    stepDescription.UpdateSpending(result.MonthlySpending);
                    stepDescription.UpdateGrowth(_growthRate);
                    stepDescription.UpdateStatePensionAmount(_statePensionAmountCalculator, personStatus, statePensionDate);
                    stepDescription.UpdatePrivatePension(_growthRate, privatePensionDate);
                    stepDescription.UpdateSalary(result.MonthlyAfterTaxSalary);
                }

                var calcMinimumStep = stepDescriptions[0].CurrentStep;
                var targetDateStep = stepDescriptions[1].CurrentStep;
                
                if (!calcdMinimum &&
                    IsThatEnoughTillDeath(calcMinimumStep, emergencyFund, personStatus, statePensionDate, privatePensionDate))
                {
                    result.MinimumPossibleRetirementDate = calcMinimumStep.Date;
                    result.SavingsAtMinimumPossiblePensionAge = Convert.ToInt32(calcMinimumStep.Savings);
                    calcdMinimum = true;
                }

                result.Steps.Add(targetDateGiven ? targetDateStep : calcMinimumStep);
            }

            UpdateResultsBasedOnSetDates(result, privatePensionDate, statePensionDate);
            result.AnnualStatePension = Convert.ToInt32(result.Steps.Last().PredictedStatePensionAnnual);
            result.PrivatePensionSafeWithdrawal = Convert.ToInt32(result.PrivatePensionPot * 0.04);
            result.StatePensionDate = statePensionDate;
            result.PrivatePensionDate = privatePensionDate;
            result.TimeToRetirement = new DateAmount(_now, result.MinimumPossibleRetirementDate);
            result.TargetRetirementDate = givenRetirementDate;
            result.TargetRetirementAge = result.TargetRetirementDate.HasValue ? AgeCalc.Age(personStatus.Dob, result.TargetRetirementDate.Value) : (int?) null;
            result.MinimumPossibleRetirementDate = result.MinimumPossibleRetirementDate;
            result.MinimumPossibleRetirementAge = AgeCalc.Age(personStatus.Dob, result.MinimumPossibleRetirementDate);
            result.StateRetirementAge = AgeCalc.Age(personStatus.Dob, result.StatePensionDate);
            result.PrivateRetirementAge = AgeCalc.Age(personStatus.Dob, result.PrivatePensionDate);

            result.Spending = Convert.ToInt32(personStatus.Spending / _monthly);

            return result;
        }

        private static void UpdateResultsBasedOnSetDates(RetirementReport result, DateTime privatePensionDate, DateTime statePensionDate)
        {
            var (privatePensionSet, statePensionSet, bankrupt) = (false, false, false);
            foreach (var step in result.Steps)
            {
                if (step.Savings < 0 && !bankrupt)
                {
                    bankrupt = true;
                    result.BankruptDate = step.Date;
                }

                if (step.Date >= privatePensionDate && !privatePensionSet) //TODO: assumes does not work past private pension date 
                {
                    privatePensionSet = true;
                    result.PrivatePensionPot = Convert.ToInt32(step.PrivatePensionAmount);
                    result.SavingsAtPrivatePensionAge = Convert.ToInt32(step.Savings);
                }

                if (step.Date >= statePensionDate && !statePensionSet)
                {
                    //TODO: FIX! in middle of another refactor but below should be statePensionSet!
                    privatePensionSet = true;
                    result.SavingsAtStatePensionAge = Convert.ToInt32(step.Savings);
                }
            }

            result.SavingsAt100 = Convert.ToInt32(result.Steps.Last().Savings);
        }

        private bool IsThatEnoughTillDeath(Step step, int minimumCash,
            PersonStatus personStatus, DateTime statePensionDate, DateTime privatePensionDate)
        {
            var monthsToDeath = MonthsToDeath(personStatus.Dob, step.Date);
            var monthlySpending = personStatus.Spending / _monthly;
            var monthlyStatePension = step.PredictedStatePensionAnnual / _monthly;
            var privatePensionAmount = step.PrivatePensionAmount;
            
            decimal runningCash = step.Savings;
            for (int month = 1; month <= monthsToDeath; month++)
            {
                runningCash -= monthlySpending;
                runningCash += runningCash * _growthRate;

                if (step.Date.AddMonths(month) > statePensionDate)
                    runningCash += monthlyStatePension;

                var pensionGrowth = privatePensionAmount * _growthRate;
                if (step.Date.AddMonths(month) > privatePensionDate)
                    runningCash += pensionGrowth;
                else
                    privatePensionAmount += pensionGrowth;

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

    public class StepDescription : IStepUpdater
    {
        private readonly StepType _stepType;
        private readonly List<Step> _steps = new List<Step>();

        public StepDescription(StepType stepType)
        {
            _stepType = stepType;
        }

        public Step CurrentStep => _steps.Last();

        public void SetInitialStep(Step step)
        {
            _steps.Add(step);
        }

        public void NewStep(PersonStatus personStatus, bool calcdMinimum, DateTime? givenRetirementDate)
        {
            _steps.Add(_stepType == StepType.CalcMinimum 
                ? new Step(_steps.Last(), personStatus, calcdMinimum) 
                : new Step(_steps.Last(), personStatus, false, givenRetirementDate)); 
        }

        public void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator, PersonStatus personStatus, DateTime statePensionDate)
        {
            _steps.Last().UpdateStatePensionAmount(statePensionAmountCalculator, personStatus, statePensionDate);
        }

        public void UpdateGrowth(decimal growthRate)
        {
            _steps.Last().UpdateGrowth(growthRate);;
        }

        public void UpdatePrivatePension(decimal growthRate, DateTime privatePensionDate)
        {
            _steps.Last().UpdatePrivatePension(growthRate, privatePensionDate);
        }

        public void UpdateSalary(decimal monthlyAfterTaxSalary)
        {
            _steps.Last().UpdateSalary(monthlyAfterTaxSalary);
        }

        public void UpdateSpending(decimal monthlySpending)
        {
            _steps.Last().UpdateSpending(monthlySpending);
        }
    }

    public enum StepType
    {
        CalcMinimum,
        GivenDate
    }
}