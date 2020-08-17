﻿using System;

namespace TaxCalculator.ExternalInterface
{
    public class PersonStatus
    {
        private decimal _employeeContribution;
        private decimal _employerContribution;

        public int ExistingSavings { get; set; }
        public int ExistingPrivatePension { get; set; }
        public int Salary { get; set; }
        public int Spending { get; set; }
        public DateTime Dob { get; set; }
        public Sex Sex { get; set; }
        public decimal MonthlySpending => Spending / 12m;

        public decimal EmployeeContribution
        {
            get => _employeeContribution;
            set
            {
                if(value>1 || value < 0)
                    throw new Exception($"Invalid employee contribution of {value} given. Must be between 0 & 1 (i.e. 0 and 100%)");
                _employeeContribution = value;
            }
        }

        public decimal EmployerContribution
        {
            get => _employerContribution;
            set
            {
                if(value>1 || value < 0)
                    throw new Exception($"Invalid employer contribution of {value} given. Must be between 0 & 1 (i.e. 0 and 100%)");
                _employerContribution = value;
            }
        }
    }
}