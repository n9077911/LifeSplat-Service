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
            var family = new FamilyStatus(personStatuses);
            var result = new RetirementReport(_pensionAgeCalc, family);


            var targetDateGiven = givenRetirementDate.HasValue;

            var emergencyFund = 0;

            foreach (var personStatus in family.Persons)
            {
                personStatus.StatePensionDate = _pensionAgeCalc.StatePensionDate(personStatus.Dob, personStatus.Sex);
                personStatus.PrivatePensionDate = _pensionAgeCalc.PrivatePensionDate(personStatus.StatePensionDate);
                personStatus.CalcMinimumSteps = new StepDescription(personStatus, StepType.CalcMinimum, _now, _growthRate);
                personStatus.TargetSteps = new StepDescription(personStatus, StepType.GivenDate, _now, _growthRate);
            }

            result.CurrentSteps(family, targetDateGiven);

            var calcdMinimum = false;

            for (var month = 1; month <= MonthsToDeath(family.PrimaryPerson.Dob, _now); month++)
            {
                foreach (var person in family.Persons)
                    foreach (var stepDescription in person.StepDescriptions)
                    {
                        stepDescription.NewStep(calcdMinimum, givenRetirementDate);
                        stepDescription.UpdateSpending(result.MonthlySpending);
                        stepDescription.UpdateGrowth(_growthRate);
                        stepDescription.UpdateStatePensionAmount(_statePensionAmountCalculator);
                        stepDescription.UpdatePrivatePension(_growthRate);
                        stepDescription.UpdateSalary(result.PersonReports[person].MonthlyAfterTaxSalary);
                    }

                family.BalanceSavings();
                
                if (!calcdMinimum && IsThatEnoughTillDeath(emergencyFund, family))
                {
                    result.MinimumPossibleRetirementDate = family.PrimaryPerson.CalcMinimumSteps.CurrentStep.Date;
                    result.SavingsAtMinimumPossiblePensionAge =  Convert.ToInt32(family.Persons.Select(status => status.CalcMinimumSteps.CurrentStep.Savings).Sum());
                    calcdMinimum = true;
                }

                result.CurrentSteps(family, targetDateGiven);
            }

            result.UpdateResultsBasedOnSetDates();
            result.UpdatePersonResults();
            result.TimeToRetirement = new DateAmount(_now, result.MinimumPossibleRetirementDate);
            result.TargetRetirementDate = givenRetirementDate;
            result.TargetRetirementAge = result.TargetRetirementDate.HasValue ? AgeCalc.Age(family.PrimaryPerson.Dob, result.TargetRetirementDate.Value) : (int?) null;
            result.MinimumPossibleRetirementDate = result.MinimumPossibleRetirementDate;
            result.MinimumPossibleRetirementAge = AgeCalc.Age(family.PrimaryPerson.Dob, result.MinimumPossibleRetirementDate);

            result.Spending = Convert.ToInt32(family.Spending / _monthly);

            return result;
        }
        
        private bool IsThatEnoughTillDeath(int minimumCash, FamilyStatus family)
        {
            var primaryStep = family.PrimaryPerson.CalcMinimumSteps.CurrentStep;
            var monthsToDeath = MonthsToDeath(family.PrimaryPerson.Dob, primaryStep.Date);
            var monthlySpending = family.Spending / _monthly;

            //todo: try refactor this out
            var privatePensionAmounts = family.Persons.Select(p => new{p, p.CalcMinimumSteps.CurrentStep.PrivatePensionAmount})
                .ToDictionary(arg => arg.p, arg=>arg.PrivatePensionAmount);

            var runningCash = family.Persons.Select(p => p.CalcMinimumSteps.CurrentStep.Savings).Sum();
            
            for (var month = 1; month <= monthsToDeath; month++)
            {
                runningCash -= monthlySpending;
                runningCash += runningCash * _growthRate;
                
                foreach (var person in family.Persons)
                {
                    if (primaryStep.Date.AddMonths(month) > person.StatePensionDate)
                        runningCash += person.CalcMinimumSteps.CurrentStep.PredictedStatePensionAnnual / _monthly;

                    var pensionGrowth = privatePensionAmounts[person] * _growthRate;
                    if (primaryStep.Date.AddMonths(month) > person.PrivatePensionDate)
                        runningCash += pensionGrowth;
                    else
                        privatePensionAmounts[person] += pensionGrowth;
                }

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
        private readonly PersonStatus _personStatus;
        private readonly StepType _stepType;
        private readonly List<Step> _steps = new List<Step>();

        public StepDescription(PersonStatus personStatus, StepType stepType, DateTime now, decimal growthRate)
        {
            _personStatus = personStatus;
            _stepType = stepType;
            _steps.Add(new Step
            {
                Date = now, Savings = personStatus.ExistingSavings,
                PrivatePensionAmount = personStatus.ExistingPrivatePension, PrivatePensionGrowth = personStatus.ExistingPrivatePension * growthRate
            });
        }

        public List<Step> Steps => _steps;
        public Step CurrentStep => _steps.Last();

        public void NewStep(bool calcdMinimum, DateTime? givenRetirementDate)
        {
            _steps.Add(_stepType == StepType.CalcMinimum 
                ? new Step(CurrentStep, _personStatus, calcdMinimum) 
                : new Step(CurrentStep, _personStatus, false, givenRetirementDate)); 
        }

        public void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator)
        {
            CurrentStep.UpdateStatePensionAmount(statePensionAmountCalculator);
        }

        public void UpdateGrowth(decimal growthRate)
        {
            CurrentStep.UpdateGrowth(growthRate);
        }

        public void UpdatePrivatePension(decimal growthRate)
        {
            CurrentStep.UpdatePrivatePension(growthRate);
        }

        public void UpdateSalary(decimal monthlyAfterTaxSalary)
        {
            CurrentStep.UpdateSalary(monthlyAfterTaxSalary);
        }

        public void UpdateSpending(decimal monthlySpending)
        {
            CurrentStep.UpdateSpending(monthlySpending);
        }

        public void SetSavings(decimal combinedSavings)
        {
            CurrentStep.Savings = combinedSavings;
        }
    }

    public enum StepType
    {
        CalcMinimum,
        GivenDate
    }
}