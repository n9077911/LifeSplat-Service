using System;
using System.Collections.Generic;
using System.Linq;
using TaxCalculator.ExternalInterface;
using TaxCalculator.TaxSystem;

namespace TaxCalculator
{
    public class RetirementIncrementalApproachCalculator : IRetirementCalculator
    {
        private readonly IAssumptions _assumptions;
        private readonly IPensionAgeCalc _pensionAgeCalc;
        private readonly IStatePensionAmountCalculator _statePensionAmountCalculator;
        private readonly decimal _monthly = 12;
        private readonly DateTime _now;
        private IncomeTaxCalculator _incomeTaxCalculator;

        public RetirementIncrementalApproachCalculator(IDateProvider dateProvider,
            IAssumptions assumptions, IPensionAgeCalc pensionAgeCalc,
            IStatePensionAmountCalculator statePensionAmountCalculator)
        {
            _assumptions = assumptions;
            _pensionAgeCalc = pensionAgeCalc;
            _statePensionAmountCalculator = statePensionAmountCalculator;
            _now = dateProvider.Now();
        }

        public IRetirementReport ReportForTargetAge(PersonStatus personStatus, IEnumerable<SpendingStepInput> spendingStepInputs, int? retirementAge = null)
        {
            return ReportForTargetAge(new[] {personStatus}, spendingStepInputs, retirementAge);
        }

        public IRetirementReport ReportForTargetAge(IEnumerable<PersonStatus> personStatus, IEnumerable<SpendingStepInput> spendingStepInputs, int? retirementAge = null)
        {
            var retirementDate = retirementAge.HasValue && retirementAge.Value != 0 ? personStatus.First().Dob.AddYears(retirementAge.Value) : (DateTime?) null;
            return ReportFor(personStatus, spendingStepInputs, retirementDate);
        }

        public IRetirementReport ReportFor(PersonStatus personStatus, IEnumerable<SpendingStepInput> spendingStepInputs, DateTime? givenRetirementDate = null)
        {
            return ReportFor(new[] {personStatus}, spendingStepInputs, givenRetirementDate);
        }

        public IRetirementReport ReportFor(IEnumerable<PersonStatus> personStatuses, IEnumerable<SpendingStepInput> spendingStepInputs, DateTime? givenRetirementDate = null)
        {
            var family = new FamilyStatus(personStatuses, spendingStepInputs);
            _incomeTaxCalculator = new IncomeTaxCalculator();
            var result = new RetirementReport(_pensionAgeCalc, _incomeTaxCalculator, family, _now, givenRetirementDate, _assumptions);

            var emergencyFund = 0;

            var calcdMinimum = false;

            for (var month = 1; month <= MonthsToDeath(family.PrimaryPerson.Dob, _now); month++)
            {
                foreach (var person in result.Persons)
                    foreach (var stepDescription in person.StepReports)
                    {
                        stepDescription.NewStep(calcdMinimum, result, result.Persons.Count(), givenRetirementDate);
                        stepDescription.UpdateSpending();
                        stepDescription.UpdateGrowth();
                        stepDescription.UpdateStatePensionAmount(_statePensionAmountCalculator, person.StatePensionDate);
                        stepDescription.UpdatePrivatePension(givenRetirementDate);
                        stepDescription.UpdateSalary(person.MonthlySalaryAfterDeductions);
                        stepDescription.ProcessTaxableIncomeIntoSavings();
                    }

                result.BalanceSavings();
                
                if (!calcdMinimum && IsThatEnoughTillDeath(emergencyFund, result))
                {
                    foreach (var resultPerson in result.Persons)
                    {
                        resultPerson.MinimumPossibleRetirementDate = result.PrimaryPerson.CalcMinimumSteps.CurrentStep.Date;
                        resultPerson.SavingsAtMinimumPossiblePensionAge =  Convert.ToInt32(result.Persons.Select(p => p.CalcMinimumSteps.CurrentStep.Savings).Sum());
                    }
                    calcdMinimum = true;
                }
            }

            result.UpdateResultsBasedOnSetDates();
            result.UpdatePersonResults();
            result.TimeToRetirement = new DateAmount(_now, result.MinimumPossibleRetirementDate);
            result.TargetRetirementDate = givenRetirementDate;
            result.TargetRetirementAge = result.TargetRetirementDate.HasValue ? AgeCalc.Age(family.PrimaryPerson.Dob, result.TargetRetirementDate.Value) : (int?) null;

            return result;
        }
        
        private bool IsThatEnoughTillDeath(int minimumCash, IRetirementReport result)
        {
            var primaryStep = result.PrimaryPerson.CalcMinimumSteps.CurrentStep;
            var monthsToDeath = MonthsToDeath(result.PrimaryPerson.Status.Dob, primaryStep.Date);

            var privatePensionAmounts = result.Persons.Select(p => new{p, p.CalcMinimumSteps.CurrentStep.PrivatePensionAmount})
                .ToDictionary(arg => arg.p, arg=>arg.PrivatePensionAmount);

            var runningCash = result.Persons.Select(p => p.CalcMinimumSteps.CurrentStep.Savings).Sum();
            
            
            for (var month = 1; month <= monthsToDeath; month++)
            {
                runningCash -= result.MonthlySpendingAt(primaryStep.Date.AddMonths(month));
                
                runningCash += runningCash * _assumptions.MonthlyGrowthRate;
                
                foreach (var person in result.Persons)
                {
                    var annualisedPrivatePensionGrowth = privatePensionAmounts[person] * _assumptions.AnnualGrowthRate;
                    var monthlyPrivatePensionGrowth = privatePensionAmounts[person] * _assumptions.MonthlyGrowthRate;
                    var annualStatePension = person.CalcMinimumSteps.CurrentStep.PredictedStatePensionAnnual;
                    
                    var taxResult = _incomeTaxCalculator.TaxFor(0, annualisedPrivatePensionGrowth, annualStatePension);

                    if (primaryStep.Date.AddMonths(month) > person.StatePensionDate)
                        runningCash += taxResult.RemainderFor(IncomeType.StatePension) / _monthly;

                    if (primaryStep.Date.AddMonths(month) > person.PrivatePensionDate)
                        runningCash += monthlyPrivatePensionGrowth - (taxResult.TotalTaxFor(IncomeType.PrivatePension) / _monthly);
                    else
                        privatePensionAmounts[person] += monthlyPrivatePensionGrowth;
                }

                if (runningCash < minimumCash)
                    return false;
            }

            return true;
        }

        private int MonthsToDeath(DateTime dob, DateTime now)
        {
            var dateAmount = new DateAmount(now, dob.AddYears(_assumptions.EstimatedDeathAge));
            return dateAmount.TotalMonths();
        }
    }
}