using System;
using System.Collections.Generic;
using System.Linq;
using Calculator.ExternalInterface;
using Calculator.Input;
using Calculator.Output;
using Calculator.StatePensionCalculator;
using Calculator.TaxSystem;

namespace Calculator
{
    /// <summary>
    /// Generates a retirement report detailing when a user can retire by iterating through a users future life
    /// </summary>
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

        public IRetirementReport ReportForTargetAge(Person person, IEnumerable<SpendingStep> spendingStepInputs, int? retirementAge = null)
        {
            return ReportForTargetAge(new[] {person}, spendingStepInputs, retirementAge);
        }

        public IRetirementReport ReportForTargetAge(IEnumerable<Person> personStatus, IEnumerable<SpendingStep> spendingStepInputs, int? retirementAge = null)
        {
            var retirementDate = retirementAge.HasValue && retirementAge.Value != 0 ? personStatus.First().Dob.AddYears(retirementAge.Value) : (DateTime?) null;
            return ReportFor(new Family(personStatus, spendingStepInputs), retirementDate);
        }

        public IRetirementReport ReportFor(Person person, IEnumerable<SpendingStep> spendingStepInputs, DateTime? givenRetirementDate = null)
        {
            IEnumerable<Person> personStatuses = new[] {person};
            return ReportFor(new Family(personStatuses, spendingStepInputs), givenRetirementDate);
        }

        public IRetirementReport ReportFor(Family family, DateTime? givenRetirementDate = null)
        {
            _incomeTaxCalculator = new IncomeTaxCalculator();
            var result = new RetirementReport(_pensionAgeCalc, _incomeTaxCalculator, family, _now, givenRetirementDate, _assumptions);

            var emergencyFund = 0;

            var calcdMinimum = false;

            for (var month = 1; month <= MonthsToDeath(family.PrimaryPerson.Dob, _now); month++)
            {
                foreach (var person in result.Persons)
                    foreach (var stepDescription in person.StepReports)
                    {
                        stepDescription.NewStep(calcdMinimum, result, result.Persons.Count, givenRetirementDate);
                        stepDescription.UpdateSpending();
                        stepDescription.UpdateGrowth();
                        stepDescription.UpdateStatePensionAmount(_statePensionAmountCalculator, person.StatePensionDate);
                        stepDescription.UpdatePrivatePension();
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
            var monthsToDeath = MonthsToDeath(result.PrimaryPerson.Person.Dob, primaryStep.Date);

            var privatePensionAmounts = result.Persons.Select(p => new{p, p.CalcMinimumSteps.CurrentStep.PrivatePensionAmount})
                .ToDictionary(arg => arg.p, arg=>arg.PrivatePensionAmount);

            var runningInvestments = result.Persons.Select(p => p.CalcMinimumSteps.CurrentStep.Savings).Sum();
            var emergencyFund = result.Persons.Select(p => p.CalcMinimumSteps.CurrentStep.EmergencyFund).Sum();
            var emergencyFundMet = false;
            
            for (var month = 1; month <= monthsToDeath; month++)
            {
                var newIncome = 0m;
                var monthlySpending = result.MonthlySpendingAt(primaryStep.Date.AddMonths(month));

                (emergencyFund, runningInvestments) = ApplySpending(monthlySpending, emergencyFund, runningInvestments);
                        
                newIncome += runningInvestments * _assumptions.MonthlyGrowthRate;
                
                foreach (var person in result.Persons)
                {
                    var annualisedPrivatePensionGrowth = privatePensionAmounts[person] * _assumptions.AnnualGrowthRate;
                    var monthlyPrivatePensionGrowth = privatePensionAmounts[person] * _assumptions.MonthlyGrowthRate;
                    var annualStatePension = person.CalcMinimumSteps.CurrentStep.PredictedStatePensionAnnual;
                    
                    var taxResult = _incomeTaxCalculator.TaxFor(0, annualisedPrivatePensionGrowth, annualStatePension);

                    if (primaryStep.Date.AddMonths(month) > person.StatePensionDate)
                        newIncome += taxResult.AfterTaxIncomeFor(IncomeType.StatePension) / _monthly;

                    if (primaryStep.Date.AddMonths(month) > person.PrivatePensionDate)
                        newIncome += monthlyPrivatePensionGrowth - (taxResult.TotalTaxFor(IncomeType.PrivatePension) / _monthly);
                    else
                        privatePensionAmounts[person] += monthlyPrivatePensionGrowth;
                }
                
                var requiredCash = result.Persons.Select(p => p.Person.EmergencyFundSpec.RequiredSavings(monthlySpending / result.Persons.Count())).Sum();
                
                (emergencyFund, runningInvestments) = AssignIncomeToRelevantPot(requiredCash, newIncome, emergencyFund, runningInvestments);
                (emergencyFund, runningInvestments) = RebalanceInvestmentsAndCashSavings(requiredCash, emergencyFund, runningInvestments);

                if (emergencyFund >= requiredCash)
                    emergencyFundMet = true;
                    
                if (emergencyFundMet && runningInvestments <= minimumCash)
                    return false;
            }

            return emergencyFundMet;
        }

        private static (decimal, decimal) ApplySpending(decimal monthlySpending, decimal emergencyFund, decimal investments)
        {
            if (investments > monthlySpending)
                investments -= monthlySpending;
            else
            {
                var diff = monthlySpending - investments;
                investments = 0;
                if (emergencyFund > diff)
                {
                    emergencyFund -= diff;
                }
                else
                {
                    var newDiff = emergencyFund - diff;
                    emergencyFund = 0;
                    investments -= newDiff;
                }
            }

            return (emergencyFund, investments);
        }

        private (decimal, decimal) RebalanceInvestmentsAndCashSavings(decimal requiredCash, decimal emergencyFund, decimal investments)
        {
            var newlyRequiredAmount = requiredCash - emergencyFund;
            if (emergencyFund < requiredCash)
            {
                if (investments > newlyRequiredAmount)
                {
                    emergencyFund = requiredCash;
                    investments -= newlyRequiredAmount;
                }
                else
                {
                    emergencyFund += investments;
                    investments = 0;
                }
            }
            else
            {
                investments += emergencyFund - requiredCash;
                emergencyFund = requiredCash;
            }

            return (emergencyFund, investments);
        }

        private static (decimal, decimal) AssignIncomeToRelevantPot(decimal requiredCash, decimal newIncome, decimal emergencyFund, decimal investments)
        {

            var newlyRequiredCash = requiredCash - emergencyFund;

            if (newlyRequiredCash > 0 && newIncome >= newlyRequiredCash) //income more than fills the cash requirement then assign the relevant amount to cash and remainder to investments
            {
                emergencyFund = requiredCash;
                investments += newIncome - newlyRequiredCash;
            }
            else if (newlyRequiredCash > 0 && newIncome < newlyRequiredCash) //income fails to fill the cash requirement then assign all income to cash.
            {
                emergencyFund += newIncome;
            }
            else
            {
                investments += newIncome;
            }

            return (emergencyFund, investments);
        }

        private int MonthsToDeath(DateTime dob, DateTime now)
        {
            var dateAmount = new DateAmount(now, dob.AddYears(_assumptions.EstimatedDeathAge));
            return dateAmount.TotalMonths();
        }
    }
}