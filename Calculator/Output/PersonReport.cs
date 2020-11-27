using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Calculator.ExternalInterface;
using Calculator.Input;
using Calculator.StatePensionCalculator;
using Calculator.TaxSystem;

namespace Calculator.Output
{
    internal class PersonReport : IPersonReport
    {
        private const decimal Monthly = 12;
        private bool _pensionCrystallised = false;
        private readonly bool _take25 = false;

        public PersonReport(IPensionAgeCalc pensionAgeCalc, IIncomeTaxCalculator incomeTaxCalculator, Person person, DateTime now, DateTime? givenRetirementDate, IAssumptions assumptions,
            decimal monthlySpending, ITaxSystem taxSystem)
        {
            Person = person;
            StatePensionDate = pensionAgeCalc.StatePensionDate(person.Dob, person.Sex);
            PrivatePensionDate = pensionAgeCalc.PrivatePensionDate(StatePensionDate, person.Dob);
            TargetRetirementDate = givenRetirementDate;
            var salaryAfterDeductions = person.EmployeeContribution.SubtractContribution(person.Salary);
            var taxResult = incomeTaxCalculator.TaxFor(salaryAfterDeductions);
            var taxResultWithRental = incomeTaxCalculator.TaxFor(salaryAfterDeductions, rentalIncome: person.RentalPortfolio.RentalIncome());

            NationalInsuranceBill = Convert.ToInt32(taxResult.NationalInsurance);
            IncomeTaxBill = Convert.ToInt32(taxResult.IncomeTax);
            RentalTaxBill = Convert.ToInt32(taxResultWithRental.TotalTaxFor(IncomeType.RentalIncome));
            TakeHomeSalary = Convert.ToInt32(taxResult.AfterTaxIncome);
            TakeHomeRentalIncome = Convert.ToInt32(person.RentalPortfolio.TotalNetIncome() - RentalTaxBill);

            SalarySteps = SetupSalarySteps(person, now).ToList();

            var niContributingYearsSoFar = NiContributingYearsCalc.CalculateContributingYearsSoFar(person, MonthlySalaryAfterDeductionsAt(now), now, taxSystem);

            var mode = givenRetirementDate.HasValue ? StepType.GivenDate : StepType.CalcMinimum;
            StepReport = new StepsReport(person, niContributingYearsSoFar, mode, now, assumptions, monthlySpending, PrivatePensionDate, taxSystem);

            _take25 = assumptions.Take25;
        }

        private static IEnumerable<SalaryStepReport> SetupSalarySteps(Person person, DateTime now)
        {
            var salaryStepInputs = new List<SalaryStep> {new SalaryStep(now, person.Salary)};
            salaryStepInputs.AddRange(person.SalaryStepInputs);
            salaryStepInputs = salaryStepInputs.OrderBy(input => input.Date).ToList();

            for (int i = 0; i < salaryStepInputs.Count; i++)
            {
                var endDate = i < salaryStepInputs.Count - 1 ? salaryStepInputs[i + 1].Date : person.Dob.AddYears(100);
                yield return new SalaryStepReport(salaryStepInputs[i].Date, endDate.AddDays(-1), salaryStepInputs[i].NewAmount);
            }
        }

        private PersonReport(Person person, in DateTime statePensionDate, in DateTime privatePensionDate, DateTime? targetRetirementDate,
            in int nationalInsuranceBill, in int incomeTaxBill, in int rentalTaxBill, in int takeHomeSalary, in int takeHomeRentalIncome, in bool take25, in bool pensionCrystallised,
            StepsReport stepReport, List<SalaryStepReport> salaryStepReports)
        {
            Person = person;
            StatePensionDate = statePensionDate;
            PrivatePensionDate = privatePensionDate;
            TargetRetirementDate = targetRetirementDate;
            NationalInsuranceBill = nationalInsuranceBill;
            IncomeTaxBill = incomeTaxBill;
            RentalTaxBill = rentalTaxBill;
            TakeHomeSalary = takeHomeSalary;
            TakeHomeRentalIncome = takeHomeRentalIncome;
            _take25 = take25;
            _pensionCrystallised = pensionCrystallised;
            StepReport = stepReport;
            SalarySteps = salaryStepReports;
        }

        public IPersonReport CopyFormCalcMinimumMode()
        {
            var personReport = new PersonReport(Person, StatePensionDate, PrivatePensionDate, TargetRetirementDate, NationalInsuranceBill, IncomeTaxBill, RentalTaxBill, TakeHomeSalary,
                TakeHomeRentalIncome, _take25, _pensionCrystallised, StepReport.CopyFromCurrent(), SalarySteps);

            return personReport;
        }

        public decimal MonthlySalaryAfterDeductionsAt(DateTime date)
        {
            var salaryStep = SalarySteps.FirstOrDefault(step => date.Date >= step.StartDate.Date && date.Date <= step.EndDate.Date) ?? SalarySteps.Last();
            var salaryAfterDeductions = Person.EmployeeContribution.SubtractContribution(salaryStep.Salary);
            return salaryAfterDeductions / Monthly;
        }

        private List<SalaryStepReport> SalarySteps { get; } = new List<SalaryStepReport>();

        public Person Person { get; }

        public StepsReport StepReport { get; }

        public int NationalInsuranceBill { get; }
        public int IncomeTaxBill { get; }
        public int RentalTaxBill { get; }
        public int TakeHomeSalary { get; }
        public int TakeHomeRentalIncome { get; }
        public int PensionContributions => Convert.ToInt32(Person.EmployeeContribution.Amount(Person.Salary) + Person.EmployerContribution.Amount(Person.Salary));

        public DateTime StatePensionDate { get; set; }
        public DateTime PrivatePensionDate { get; }
        public DateTime PrivatePensionPotCrystallisationDate{ get; set; }
        public DateTime? TargetRetirementDate { get; }
        public DateTime FinancialIndependenceDate { get; set; }

        public int BankruptAge { get; set; }
        public int AnnualStatePension { get; set; }
        public int NiContributingYears { get; set; }
        public int PrivatePensionSafeWithdrawal { get; set; }

        public int PrivatePensionCrystallisationAge { get; set; }
        public int FinancialIndependenceAge => AgeCalc.Age(Person.Dob, FinancialIndependenceDate);
        public int StatePensionAge => AgeCalc.Age(Person.Dob, StatePensionDate);
        public int PrivatePensionAge => AgeCalc.Age(Person.Dob, PrivatePensionDate);
        public int? TargetRetirementAge => TargetRetirementDate.HasValue ? AgeCalc.Age(Person.Dob, TargetRetirementDate.Value) : (int?)null;
        
        
        public int SavingsAtFinancialIndependenceAge { get; set; }
        public int PrivatePensionPotAtFinancialIndependenceAge { get; set; }        
        public int? SavingsAtTargetRetirementAge { get; set; }
        public int? PrivatePensionPotAtTargetRetirementAge { get; set; }
        public int PrivatePensionPotAtCrystallisationAge { get; set; }
        public int PrivatePensionPotBeforeCrystallisation { get; set; }
        
        public int Take25LumpSum { get; set; }
        public int LifeTimeAllowanceTaxCharge { get; set; }

        public bool Retired(in bool calcdMinimum, in DateTime now, DateTime? givenRetirementDate)
        {
            if (!_pensionCrystallised &&
                ((calcdMinimum && now > PrivatePensionDate) 
                 || (givenRetirementDate != null && now > givenRetirementDate  && now > PrivatePensionDate)))
            {
                return true;
            }

            return false;
        }

        public void CrystallisePension()
        {
            PrivatePensionPotBeforeCrystallisation = Convert.ToInt32(StepReport.CurrentStep.PrivatePensionAmount);

            var ltaCharge = StepReport.CalcLtaCharge();
            StepReport.PayLtaCharge(ltaCharge);
            LifeTimeAllowanceTaxCharge = Convert.ToInt32(ltaCharge);

            if (_take25)
            {
                var take25Result = StepReport.CalcTake25();
                StepReport.Take25(take25Result);
                Take25LumpSum = Convert.ToInt32(take25Result.TaxFreeAmount);
            }
            
            PrivatePensionPotAtCrystallisationAge = Convert.ToInt32(StepReport.CurrentStep.PrivatePensionAmount);
            PrivatePensionPotCrystallisationDate = StepReport.CurrentStep.StepDate;
            PrivatePensionCrystallisationAge = AgeCalc.Age(Person.Dob, PrivatePensionPotCrystallisationDate);
            _pensionCrystallised = true;
        }

        public void UpdateFinancialIndependenceDate(in DateTime minimumPossibleRetirementDate)
        {
            FinancialIndependenceDate = minimumPossibleRetirementDate;
        }

        public void UpdateWithConclusions(IAssumptions assumptions, DateTime bankruptDate)
        {
            PrivatePensionSafeWithdrawal = Convert.ToInt32(PrivatePensionPotAtCrystallisationAge * assumptions.AnnualGrowthRate); 
            AnnualStatePension = Convert.ToInt32(StepReport.Steps.Last().PredictedStatePensionAnnual);
            NiContributingYears = StepReport.Steps.Last().NiContributingYears;
            StatePensionDate = StatePensionDate;
            
            BankruptAge = AgeCalc.Age(Person.Dob, bankruptDate);
        }
    }
}