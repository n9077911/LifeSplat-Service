using System;
using System.Collections.Generic;
using TaxCalculator;

namespace TaxCalcService.Models.DTO
{
    public class RetirementReportDto
    {
        public int MinimumPossibleRetirementAge { get; }
        public int? TargetRetirementAge { get; }
        public DateTime MinimumPossibleRetirementDate{ get; }
        public DateTime? TargetRetirementDate{ get; }
        public string TimeToRetirementDescription { get; }
        public int Spending { get; }
        public int SavingsAtPrivatePensionAge { get; }
        public int SavingsAtStatePensionAge { get; }
        public DateTime BankruptDate { get; }
        public List<string> StepsHeaders { get; }
        public List<PersonReportDto> Person = new List<PersonReportDto>(); 
        public int PrivatePensionPotAtStatePensionAge { get; }
        public int PrivatePensionPotAtPrivatePensionAge { get; }
        
        public RetirementReportDto(IRetirementReport retirementReport)
        {
            MinimumPossibleRetirementAge = retirementReport.MinimumPossibleRetirementAge;
            MinimumPossibleRetirementDate = retirementReport.MinimumPossibleRetirementDate;
            TargetRetirementAge = retirementReport.TargetRetirementAge;
            TargetRetirementDate = retirementReport.TargetRetirementDate;
            TimeToRetirementDescription = retirementReport.TimeToRetirement.ToString();
            Spending = retirementReport.Spending;
            
            SavingsAtPrivatePensionAge = retirementReport.SavingsAtPrivatePensionAge;
            SavingsAtStatePensionAge = retirementReport.SavingsAtStatePensionAge;
            PrivatePensionPotAtPrivatePensionAge = retirementReport.PrivatePensionPotAtPrivatePensionAge;
            PrivatePensionPotAtStatePensionAge = retirementReport.PrivatePensionPotAtStatePensionAge;
            BankruptDate = retirementReport.BankruptDate;

            foreach (var personReport in retirementReport.PersonReports)
            {
                var steps = new List<List<object>>();
                foreach (var step in retirementReport.StepsDict[personReport.Key])
                {
                    steps.Add(new List<object>
                        {
                            step.Date.ToString("yyyy-MM-dd"),
                            Decimal.Round(step.Savings),
                            Decimal.Round(step.StatePension),
                            Decimal.Round(step.AfterTaxSalary),
                            Decimal.Round(step.Growth),
                            Decimal.Round(step.PrivatePensionGrowth),
                            Decimal.Round(step.PrivatePensionAmount),
                        }
                    );
                }
                
                Person.Add(new PersonReportDto
                {
                    StateRetirementAge = personReport.Value.StateRetirementAge,
                    PrivateRetirementAge = personReport.Value.PrivateRetirementAge,
                    StateRetirementDate = personReport.Value.StatePensionDate,
                    PrivateRetirementDate = personReport.Value.PrivatePensionDate,
                    NationalInsuranceBill = personReport.Value.NationalInsuranceBill,
                    IncomeTaxBill = personReport.Value.IncomeTaxBill,
                    AnnualStatePension = personReport.Value.AnnualStatePension,
                    PrivatePensionPot = personReport.Value.PrivatePensionPot.Value,
                    PrivatePensionSafeWithdrawal = personReport.Value.PrivatePensionSafeWithdrawal,
                    AfterTaxSalary = retirementReport.PrimaryPerson.AfterTaxSalary,
                    Steps = steps,
                });
            }

            StepsHeaders = new List<string>
            {
                "Date", "Cash", "StatePension", "AfterTaxSalary", "Growth", "PrivatePensionGrowth",
                "PrivatePensionAmount"
            };
        }
    }

    public class PersonReportDto
    {
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
    }
}