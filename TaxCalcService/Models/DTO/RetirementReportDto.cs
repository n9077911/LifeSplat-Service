using System;
using System.Collections.Generic;
using TaxCalculator.ExternalInterface;

namespace TaxCalcService.Models.DTO
{
    public class RetirementReportDto
    {
        public int RetirementAge { get; }
        public int StateRetirementAge { get; }
        public int PrivateRetirementAge { get; }
        public int TargetSavings { get; }
        public DateTime RetirementDate { get; }
        public DateTime StateRetirementDate { get; }
        public DateTime PrivateRetirementDate { get; }
        public string TimeToRetirementDescription { get; }
        public int AfterTaxSalary { get; }
        public int Spending { get; }
        public int AnnualStatePension { get; }
        public int PrivatePensionPot { get; }
        public int PrivatePensionSafeWithdrawal { get; }
        public int SavingsAtPrivatePensionAge { get; }
        public int SavingsAtStatePensionAge { get; }
        public int NationalInsuranceBill { get; }
        public int IncomeTaxBill { get; }
        public List<string> StepsHeaders { get; }
        public List<List<object>> Steps { get; }

        public RetirementReportDto(IRetirementReport retirementReport)
        {
            RetirementAge = retirementReport.RetirementAge;
            StateRetirementAge = retirementReport.StateRetirementAge;
            PrivateRetirementAge = retirementReport.PrivateRetirementAge;
            TargetSavings = retirementReport.TargetSavings;
            RetirementDate = retirementReport.MinimumPossibleRetirementDate;
            StateRetirementDate = retirementReport.StateRetirementDate;
            PrivateRetirementDate = retirementReport.PrivateRetirementDate;
            TimeToRetirementDescription = retirementReport.TimeToRetirement.ToString();
            AfterTaxSalary = retirementReport.AfterTaxSalary;
            Spending = retirementReport.Spending;
            NationalInsuranceBill = retirementReport.NationalInsuranceBill;
            IncomeTaxBill = retirementReport.IncomeTaxBill;
            AnnualStatePension = retirementReport.AnnualStatePension;
            PrivatePensionPot = retirementReport.PrivatePensionPot;
            
            PrivatePensionSafeWithdrawal = retirementReport.PrivatePensionSafeWithdrawal;
            SavingsAtPrivatePensionAge = retirementReport.SavingsAtPrivatePensionAge;
            SavingsAtStatePensionAge = retirementReport.SavingsAtStatePensionAge;

            StepsHeaders = new List<string>
            {
                "Date", "Cash", "StatePension", "AfterTaxSalary", "Growth", "PrivatePensionGrowth",
                "PrivatePensionAmount"
            };
            Steps = new List<List<object>>();
            foreach (var step in retirementReport.Steps)
            {
                Steps.Add(new List<object>
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
        }
    }
}