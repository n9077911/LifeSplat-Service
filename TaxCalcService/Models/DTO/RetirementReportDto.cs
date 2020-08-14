﻿using System;
using System.Collections.Generic;
using TaxCalculator;

namespace TaxCalcService.Models.DTO
{
    public class RetirementReportDto
    {
        public int MinimumPossibleRetirementAge { get; }
        public int? TargetRetirementAge { get; }
        public int StateRetirementAge { get; }
        public int PrivateRetirementAge { get; }
        public DateTime MinimumPossibleRetirementDate{ get; }
        public DateTime? TargetRetirementDate{ get; }
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
        public DateTime BankruptDate { get; }
        public int NationalInsuranceBill { get; }
        public int IncomeTaxBill { get; }
        public List<string> StepsHeaders { get; }
        public List<List<object>> Steps { get; }

        public RetirementReportDto(IRetirementReport retirementReport)
        {
            MinimumPossibleRetirementAge = retirementReport.MinimumPossibleRetirementAge;
            StateRetirementAge = retirementReport.PrimaryPerson.StateRetirementAge;
            PrivateRetirementAge = retirementReport.PrimaryPerson.PrivateRetirementAge;
            MinimumPossibleRetirementDate = retirementReport.MinimumPossibleRetirementDate;
            TargetRetirementAge = retirementReport.TargetRetirementAge;
            TargetRetirementDate = retirementReport.TargetRetirementDate;
            StateRetirementDate = retirementReport.PrimaryPerson.StatePensionDate;
            PrivateRetirementDate = retirementReport.PrimaryPerson.PrivatePensionDate;
            TimeToRetirementDescription = retirementReport.TimeToRetirement.ToString();
            AfterTaxSalary = retirementReport.PrimaryPerson.AfterTaxSalary;
            Spending = retirementReport.Spending;
            NationalInsuranceBill = retirementReport.PrimaryPerson.NationalInsuranceBill;
            IncomeTaxBill = retirementReport.PrimaryPerson.IncomeTaxBill;
            AnnualStatePension = retirementReport.PrimaryPerson.AnnualStatePension;
            PrivatePensionPot = retirementReport.PrimaryPerson.PrivatePensionPot.Value;
            
            PrivatePensionSafeWithdrawal = retirementReport.PrimaryPerson.PrivatePensionSafeWithdrawal;
            SavingsAtPrivatePensionAge = retirementReport.SavingsAtPrivatePensionAge;
            SavingsAtStatePensionAge = retirementReport.SavingsAtStatePensionAge;
            BankruptDate = retirementReport.BankruptDate;

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