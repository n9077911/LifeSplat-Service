using System;
using System.Collections.Generic;

namespace TaxCalculator.ExternalInterface
{
    public class PersonStatus
    {
        public int ExistingSavings { get; set; }
        public int ExistingPrivatePension { get; set; }
        public decimal EmployeeContribution { get; set; }
        public decimal EmployerContribution { get; set; }
        public int Salary { get; set; }
        public int Spending { get; set; }
        public DateTime Dob { get; set; }
        public Sex Sex { get; set; }
        public decimal MonthlySpending => Spending / 12m;

        //Added after initial creation// TODO: factor out to a separate object?
        public DateTime StatePensionDate { get; set; }
        public DateTime PrivatePensionDate { get; set; }
        public List<StepDescription> StepDescriptions => new List<StepDescription>(){CalcMinimumSteps, TargetSteps};
        public StepDescription CalcMinimumSteps { get; set; }
        public StepDescription TargetSteps { get; set; }
    }
}