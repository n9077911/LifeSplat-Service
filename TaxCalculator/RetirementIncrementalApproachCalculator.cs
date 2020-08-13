using System;
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
            return (decimal)Math.Pow((double)(1+rate), 1/(double)_monthly)-1;
        }

        public IRetirementReport ReportForTargetAge(PersonStatus personStatus, int? retirementAge = null)
        {
            return ReportFor(personStatus, retirementAge.HasValue && retirementAge.Value != 0 ? personStatus.Dob.AddYears(retirementAge.Value) : (DateTime?)null);
        }

        public IRetirementReport ReportFor(PersonStatus personStatus, DateTime? givenRetirementDate = null)
        {
            var result = new RetirementReport();
            var statePensionDate = _pensionAgeCalc.StatePensionDate(personStatus.Dob, personStatus.Sex);
            var privatePensionDate = _pensionAgeCalc.PrivatePensionDate(statePensionDate);

            var targetDateGiven = givenRetirementDate.HasValue;

            var emergencyFund = 0;
            var taxResult = new IncomeTaxCalculator().TaxFor(personStatus.Salary*(1-personStatus.EmployeeContribution));
            var monthlyAfterTaxSalary = taxResult.Remainder / _monthly;
            var monthlySpending = personStatus.Spending / _monthly;

            var previousStep = new Step {Date = _now, Savings = personStatus.ExistingSavings, 
                PrivatePensionAmount = personStatus.ExistingPrivatePension, PrivatePensionGrowth = personStatus.ExistingPrivatePension*_growthRate};
            var previousStepTargetDate = new Step {Date = _now, Savings = personStatus.ExistingSavings, 
                PrivatePensionAmount = personStatus.ExistingPrivatePension, PrivatePensionGrowth = personStatus.ExistingPrivatePension*_growthRate};

            result.Steps.Add(targetDateGiven ? previousStepTargetDate : previousStep );
            
            var calcdMinimum = false;
            
            for (var month = 1; month <= MonthsToDeath(personStatus.Dob, _now); month++)
            {
                var step = new Step(previousStep, personStatus, calcdMinimum);
                var stepTargetDate = new Step(previousStepTargetDate, personStatus, false, givenRetirementDate);

                step.UpdateSpending(monthlySpending);
                stepTargetDate.UpdateSpending(monthlySpending);

                step.UpdateGrowth(_growthRate);
                stepTargetDate.UpdateGrowth(_growthRate);
                
                step.UpdateStatePensionAmount(_statePensionAmountCalculator, personStatus, statePensionDate);
                stepTargetDate.UpdateStatePensionAmount(_statePensionAmountCalculator, personStatus, statePensionDate);
                
                step.UpdatePrivatePension(_growthRate, privatePensionDate);
                stepTargetDate.UpdatePrivatePension(_growthRate, privatePensionDate);
                
                step.UpdateSalary(monthlyAfterTaxSalary);
                stepTargetDate.UpdateSalary(monthlyAfterTaxSalary);
                
                
                if (!calcdMinimum &&
                    IsThatEnoughTillDeath(step.Savings, step.Date, emergencyFund, personStatus, statePensionDate, step.PredictedStatePensionAnnual, privatePensionDate, step.PrivatePensionAmount))
                {
                    result.MinimumPossibleRetirementDate = step.Date;
                    result.SavingsAtMinimumPossiblePensionAge = Convert.ToInt32(step.Savings);
                    calcdMinimum = true;
                }
                
                result.Steps.Add(targetDateGiven ? stepTargetDate : step);
                
                previousStep = step;
                previousStepTargetDate = stepTargetDate;
            }

            UpdateResultsBasedOnSetDates(result, privatePensionDate, statePensionDate);
            result.AnnualStatePension = Convert.ToInt32(result.Steps.Last().PredictedStatePensionAnnual);
            result.PrivatePensionSafeWithdrawal = Convert.ToInt32(result.PrivatePensionPot * 0.04);
            result.StateRetirementDate = statePensionDate;
            result.PrivateRetirementDate = privatePensionDate;
            result.TimeToRetirement = new DateAmount(_now, result.MinimumPossibleRetirementDate);
            result.TargetRetirementDate = givenRetirementDate;
            result.TargetRetirementAge = result.TargetRetirementDate.HasValue ? AgeCalc.Age(personStatus.Dob, result.TargetRetirementDate.Value) : (int?)null;
            result.MinimumPossibleRetirementDate = result.MinimumPossibleRetirementDate;
            result.MinimumPossibleRetirementAge = AgeCalc.Age(personStatus.Dob, result.MinimumPossibleRetirementDate);
            result.StateRetirementAge = AgeCalc.Age(personStatus.Dob, result.StateRetirementDate);
            result.PrivateRetirementAge = AgeCalc.Age(personStatus.Dob, result.PrivateRetirementDate);
            result.AfterTaxSalary = Convert.ToInt32(taxResult.Remainder*(1-personStatus.EmployeeContribution));
            result.NationalInsuranceBill = Convert.ToInt32(taxResult.NationalInsurance);
            result.IncomeTaxBill = Convert.ToInt32(taxResult.IncomeTax);
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
                    privatePensionSet = true;
                    result.SavingsAtStatePensionAge = Convert.ToInt32(step.Savings);
                }
            }

            result.SavingsAt100 = Convert.ToInt32(result.Steps.Last().Savings);
        }

        private bool IsThatEnoughTillDeath(decimal cash, DateTime now, int minimumCash,
            PersonStatus personStatus, DateTime statePensionDate, decimal statePensionAmount, DateTime privatePensionDate, decimal privatePensionAmount)
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

                var pensionGrowth = privatePensionAmount * _growthRate;
                if(now.AddMonths(month) > privatePensionDate)
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
}