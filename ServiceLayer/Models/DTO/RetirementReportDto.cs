using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Calculator.Output;

namespace ServiceLayer.Models.DTO
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class RetirementReportDto
    {
        public int? TargetRetirementAge { get; }
        public DateTime? TargetRetirementDate{ get; }
        public DateTime BankruptDate { get; }
        public int SavingsCombinedAtFinancialIndependenceAge { get; }
        public int PrivatePensionCombinedAtFinancialIndependenceAge { get; }
        public int? SavingsCombinedAtTargetRetirementAge { get; }
        public int? PrivatePensionCombinedAtTargetRetirementAge { get; }
        public int CurrentSavingsRate { get; }
        public List<string> StepsHeaders { get; }
        public List<PersonReportDto> Person { get; } = new List<PersonReportDto>();
        public List<SpendingStepDto> SpendingSteps {get;}

        public RetirementReportDto(IRetirementReport retirementReport)
        {
            TargetRetirementAge = retirementReport.TargetRetirementAge;
            TargetRetirementDate = retirementReport.TargetRetirementDate;
            BankruptDate = retirementReport.BankruptDate;
            SpendingSteps = retirementReport.SpendingSteps.Select(s => new SpendingStepDto{StartDate = s.StartDate, EndDate = s.EndDate, Spending = s.Spending}).ToList();
            SavingsCombinedAtFinancialIndependenceAge = retirementReport.SavingsCombinedAtFinancialIndependenceAge;
            PrivatePensionCombinedAtFinancialIndependenceAge = retirementReport.PrivatePensionCombinedAtFinancialIndependenceAge;
            SavingsCombinedAtTargetRetirementAge = retirementReport.SavingsCombinedAtTargetRetirementAge;
            PrivatePensionCombinedAtTargetRetirementAge = retirementReport.PrivatePensionCombinedAtTargetRetirementAge;
            CurrentSavingsRate = retirementReport.CurrentSavingsRate();
            
            foreach (var personReport in retirementReport.Persons)
            {
                var steps = new List<List<object>>();
                foreach (var step in personReport.StepReport.Steps)
                {
                    steps.Add(new List<object>
                        {
                            step.Date.ToString("yyyy-MM-dd"),
                            Decimal.Round(step.Investments),
                            Decimal.Round(step.EmergencyFund),
                            Decimal.Round(step.AfterTaxStatePension),
                            Decimal.Round(step.AfterTaxSalary),
                            Decimal.Round(step.AfterTaxRentalIncome),
                            Decimal.Round(step.Growth),
                            Decimal.Round(step.AfterTaxPrivatePensionIncome),
                            Decimal.Round(step.PrivatePensionAmount),
                            Decimal.Round(step.ChildBenefit),
                            Decimal.Round(step.Spending),
                        }
                    );
                }
                
                Person.Add(new PersonReportDto
                {
                    MinimumPossibleRetirementAge = personReport.FinancialIndependenceAge,
                    MinimumPossibleRetirementDate = personReport.FinancialIndependenceDate,
                    
                    BankruptAge = personReport.BankruptAge,
                    StateRetirementAge = personReport.StatePensionAge,
                    PrivateRetirementAge = personReport.PrivatePensionAge,
                    TargetRetirementAge = personReport.TargetRetirementAge,
                    PrivateRetirementCrystallisationAge = personReport.PrivatePensionCrystallisationAge,
                    StateRetirementDate = personReport.StatePensionDate,
                    PrivateRetirementDate = personReport.PrivatePensionDate,
                    PrivateRetirementCrystallisationDate = personReport.PrivatePensionPotCrystallisationDate,
                    NationalInsuranceBill = personReport.NationalInsuranceBill,
                    IncomeTaxBill = personReport.IncomeTaxBill,
                    RentalTaxBill = personReport.RentalTaxBill,
                    AnnualStatePension = personReport.AnnualStatePension,
                    CalculatedNiContributingYears = personReport.NiContributingYears,
                    GivenNiContributingYears = personReport.Person.NiContributingYears,
                    PrivatePensionPotAtCrystallisation = personReport.PrivatePensionPotAtCrystallisationAge,
                    PrivatePensionPotBeforeCrystallisation = personReport.PrivatePensionPotBeforeCrystallisation,
                    PrivatePensionSafeWithdrawal = personReport.PrivatePensionSafeWithdrawal,
                    Take25LumpSum = personReport.Take25LumpSum,
                    LifeTimeAllowanceTaxCharge = personReport.LifeTimeAllowanceTaxCharge,
                    AfterTaxSalary = personReport.TakeHomeSalary,
                    PensionContributions = personReport.PensionContributions,
                    TakeHomeRentalIncome = personReport.TakeHomeRentalIncome,
                    
                    Steps = steps,
                });
            }

            StepsHeaders = new List<string>
            {
                "Date", "Cash", "CashSavings", "StatePension", "AfterTaxSalary", "AfterTaxRentalIncome", "Growth", "PrivatePensionGrowth",
                "PrivatePensionAmount", "ChildBenefit", "Spending"
            };
        }
    }

    public class PersonReportDto
    {
        public DateTime MinimumPossibleRetirementDate{ get; set; }
        public int MinimumPossibleRetirementAge { get; set; }
        
        public int BankruptAge { get; set; }
        public int StateRetirementAge { get; set; }
        public int PrivateRetirementAge { get; set;}
        public int PrivateRetirementCrystallisationAge { get; set;}
        public DateTime StateRetirementDate { get; set;}
        public DateTime PrivateRetirementDate { get; set;}
        public DateTime PrivateRetirementCrystallisationDate { get; set;}
        public int AnnualStatePension { get; set;}
        public int PrivatePensionPotAtCrystallisation { get; set;}
        public int PrivatePensionPotBeforeCrystallisation { get; set;}
        public int PrivatePensionSafeWithdrawal { get; set;}
        public int Take25LumpSum { get; set;}
        public int LifeTimeAllowanceTaxCharge { get; set;}
        public int NationalInsuranceBill { get; set;}
        public int IncomeTaxBill { get; set;}
        public int RentalTaxBill { get; set;}
        public int AfterTaxSalary { get; set; }
        public int PensionContributions { get; set; }
        public int TakeHomeRentalIncome { get; set; }
        public List<List<object>> Steps { get; set; }
        public int CalculatedNiContributingYears { get; set; }
        public int? GivenNiContributingYears { get; set; }
        public int? TargetRetirementAge { get; set; }
    }
}