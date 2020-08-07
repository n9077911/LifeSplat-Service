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
            return ReportFor(personStatus, retirementAge.HasValue ? personStatus.Dob.AddYears(retirementAge.Value) : (DateTime?)null);
        }

        public IRetirementReport ReportFor(PersonStatus personStatus, DateTime? givenRetirementDate = null)
        {
            var result = new RetirementReport();
            var statePensionDate = _pensionAgeCalc.StatePensionDate(personStatus.Dob, personStatus.Sex);
            var privatePensionDate = _pensionAgeCalc.PrivatePensionDate(statePensionDate);
            var privatePensionAmount = (decimal)personStatus.ExistingPrivatePension;

            var targetDateGiven = givenRetirementDate.HasValue;

            var minimumCash = 0;
            var taxResult = new IncomeTaxCalculator().TaxFor(personStatus.Salary*(1-personStatus.EmployeeContribution));
            var monthlyAfterTaxSalary = taxResult.Remainder / _monthly;
            var monthlySpending = personStatus.Spending / _monthly;

            var previousStep = new Step {Date = _now, Savings = personStatus.ExistingSavings, 
                PrivatePensionAmount = personStatus.ExistingPrivatePension, PrivatePensionGrowth = personStatus.ExistingPrivatePension*_growthRate};

            result.Steps.Add(previousStep);
            var calcdRetirementDate = false;
            
            for (var month = 1; month <= MonthsToDeath(personStatus.Dob, _now); month++)
            {
                var stepDate = previousStep.Date.AddMonths(1);
                
                decimal stepStatePensionAmount;
                if ((!targetDateGiven && !calcdRetirementDate) || (targetDateGiven && stepDate <= givenRetirementDate))
                    stepStatePensionAmount = _statePensionAmountCalculator.Calculate(personStatus, stepDate);
                else
                    stepStatePensionAmount = result.AnnualStatePension;

                var step = new Step { Date = stepDate };

                var savings = previousStep.Savings - monthlySpending;

                var growth = savings * _growthRate;
                step.Growth = growth;
                savings += growth;

                if (stepDate > statePensionDate)
                {
                    savings += stepStatePensionAmount / _monthly;
                    step.StatePension = stepStatePensionAmount / _monthly;
                }

                var privatePensionGrowth = privatePensionAmount * _growthRate;

                if (stepDate >= privatePensionDate)
                    savings += privatePensionGrowth;
                else
                    privatePensionAmount += privatePensionGrowth;
                
                if ((!targetDateGiven && !calcdRetirementDate)  || (targetDateGiven && stepDate <= givenRetirementDate))
                {
                    savings += monthlyAfterTaxSalary;
                    step.AfterTaxSalary = monthlyAfterTaxSalary;
                    privatePensionAmount += (personStatus.Salary / _monthly) * (personStatus.EmployeeContribution + personStatus.EmployerContribution);
                }
                
                step.PrivatePensionGrowth = privatePensionGrowth;
                step.PrivatePensionAmount = privatePensionAmount;

                step.Savings = savings;
                result.Steps.Add(step);
                previousStep = step;

                if (!calcdRetirementDate &&
                    IsThatEnoughTillDeath(step.Savings, step.Date, minimumCash, personStatus, statePensionDate, stepStatePensionAmount, privatePensionDate, step.PrivatePensionAmount))
                {
                    if(!targetDateGiven)
                        result.AnnualStatePension = Convert.ToInt32(stepStatePensionAmount);
                    result.SavingsAtMinimumPossiblePensionAge = Convert.ToInt32(step.Savings);
                    result.MinimumPossibleRetirementDate = step.Date;
                    calcdRetirementDate = true;
                }
                
                if(targetDateGiven && givenRetirementDate>stepDate)
                    result.AnnualStatePension = Convert.ToInt32(stepStatePensionAmount);

                if (stepDate >= privatePensionDate && stepDate.AddMonths(-1) < privatePensionDate)
                {
                    result.PrivatePensionPot = Convert.ToInt32(privatePensionAmount);
                    result.SavingsAtPrivatePensionAge = Convert.ToInt32(step.Savings);
                }
                
                if (stepDate >= statePensionDate && stepDate.AddMonths(-1) < statePensionDate)
                {
                    result.SavingsAtStatePensionAge = Convert.ToInt32(step.Savings);
                }
            }

            result.SavingsAt100 = Convert.ToInt32(result.Steps.Last().Savings);
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