using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Calculator;
using Calculator.Output;

namespace ServiceLayer.Models.DTO
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class RetirementReportDto
    {
        public int? TargetRetirementAge { get; }
        public DateTime? TargetRetirementDate{ get; }
        public DateTime BankruptDate { get; }

        public List<string> StepsHeaders { get; }
        public List<PersonReportDto> Person { get; } = new List<PersonReportDto>();
        public List<SpendingStepDto> SpendingSteps {get;}

        public RetirementReportDto(IRetirementReport retirementReport)
        {
            TargetRetirementAge = retirementReport.TargetRetirementAge;
            TargetRetirementDate = retirementReport.TargetRetirementDate;
            BankruptDate = retirementReport.BankruptDate;
            SpendingSteps = retirementReport.SpendingSteps.Select(s => new SpendingStepDto{StartDate = s.StartDate, EndDate = s.EndDate, Spending = s.Spending}).ToList();
            
            foreach (var personReport in retirementReport.Persons)
            {
                var steps = new List<List<object>>();
                foreach (var step in personReport.PrimarySteps.Steps)
                {
                    steps.Add(new List<object>
                        {
                            step.Date.ToString("yyyy-MM-dd"),
                            Decimal.Round(step.Savings),
                            Decimal.Round(step.AfterTaxStatePension),
                            Decimal.Round(step.AfterTaxSalary),
                            Decimal.Round(step.Growth),
                            Decimal.Round(step.AfterTaxPrivatePensionIncome),
                            Decimal.Round(step.PrivatePensionAmount),
                            Decimal.Round(step.Spending),
                        }
                    );
                }
                
                Person.Add(new PersonReportDto
                {
                    MinimumPossibleRetirementAge = personReport.MinimumPossibleRetirementAge,
                    MinimumPossibleRetirementDate = personReport.MinimumPossibleRetirementDate,
                    SavingsCombinedAtPrivatePensionAge = personReport.SavingsCombinedAtPrivatePensionAge,
                    SavingsCombinedAtStatePensionAge = personReport.SavingsCombinedAtStatePensionAge,
                    PrivatePensionPotCombinedAtPrivatePensionAge = personReport.PrivatePensionPotCombinedAtPrivatePensionAge,
                    PrivatePensionPotCombinedAtStatePensionAge = personReport.PrivatePensionPotCombinedAtStatePensionAge,
                    
                    BankruptAge = personReport.BankruptAge,
                    StateRetirementAge = personReport.StatePensionAge,
                    PrivateRetirementAge = personReport.PrivatePensionAge,
                    StateRetirementDate = personReport.StatePensionDate,
                    PrivateRetirementDate = personReport.PrivatePensionDate,
                    NationalInsuranceBill = personReport.NationalInsuranceBill,
                    IncomeTaxBill = personReport.IncomeTaxBill,
                    AnnualStatePension = personReport.AnnualStatePension,
                    CalculatedNiContributingYears = personReport.NiContributingYears,
                    GivenNiContributingYears = personReport.Person.NiContributingYears,
                    PrivatePensionPot = personReport.PrivatePensionPotAtPrivatePensionAge,
                    PrivatePensionSafeWithdrawal = personReport.PrivatePensionSafeWithdrawal,
                    AfterTaxSalary = personReport.AfterTaxSalary,
                    Steps = steps,
                });
            }

            StepsHeaders = new List<string>
            {
                "Date", "Cash", "StatePension", "AfterTaxSalary", "Growth", "PrivatePensionGrowth",
                "PrivatePensionAmount", "Spending"
            };
        }
    }

    public class PersonReportDto
    {
        public DateTime MinimumPossibleRetirementDate{ get; set; }
        public int MinimumPossibleRetirementAge { get; set; }
        public int SavingsCombinedAtPrivatePensionAge { get; set; }
        public int SavingsCombinedAtStatePensionAge { get; set; }
        public int PrivatePensionPotCombinedAtStatePensionAge { get; set; }
        public int PrivatePensionPotCombinedAtPrivatePensionAge { get; set; }
        
        public int BankruptAge { get; set; }
        public int StateRetirementAge { get; set; }
        public int PrivateRetirementAge { get; set;}
        public DateTime StateRetirementDate { get; set;}
        public DateTime PrivateRetirementDate { get; set;}
        public int AnnualStatePension { get; set;}
        public int PrivatePensionPot { get; set;}
        public int PrivatePensionSafeWithdrawal { get; set;}
        public int NationalInsuranceBill { get; set;}
        public int IncomeTaxBill { get; set;}
        public int AfterTaxSalary { get; set; }
        public List<List<object>> Steps { get; set; }
        public int CalculatedNiContributingYears { get; set; }
        public int? GivenNiContributingYears { get; set; }
    }
}