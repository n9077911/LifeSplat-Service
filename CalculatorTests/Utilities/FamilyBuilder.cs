using System;
using System.Collections.Generic;
using Calculator.Input;

namespace CalculatorTests.Utilities
{
    public class FamilyBuilder
    {
        public Person[] Persons { get; set; }
        public SpendingStep[] Spending { get; set; }
        private List<RentalInfo> RentalInfo0 { get; } = new List<RentalInfo>();
        private List<RentalInfo> RentalInfo1 { get; } = new List<RentalInfo>();

        public FamilyBuilder WithRental(int personIndex, int rent, int mortgagePayments, int expenses)
        {
            if(personIndex == 0)
                RentalInfo0.Add(new RentalInfo() {Expenses = expenses, GrossIncome = rent, MortgagePayments = mortgagePayments});
            if(personIndex == 1)
                RentalInfo1.Add(new RentalInfo() {Expenses = expenses, GrossIncome = rent, MortgagePayments = mortgagePayments});

            return this;
        }

        public Family Family()
        {
            Persons[0].RentalPortfolio = new RentalPortfolio(RentalInfo0);
            Persons[1].RentalPortfolio = new RentalPortfolio(RentalInfo1);
            return new Family(Persons, Spending);
        }

        public FamilyBuilder WithChild(DateTime dob)
        {
            Persons[0].Children.Add(dob);
            return this;
        }

        public FamilyBuilder WithSalary(int salary1, int salary2)
        {
            Persons[0].Salary = salary1;
            Persons[1].Salary = salary2;
            return this;
        }

        public FamilyBuilder WithEmployeePension(decimal pension1, decimal pension2)
        {
            Persons[0].EmployeeContribution = pension1;
            Persons[1].EmployeeContribution = pension2;
            return this;
        }
    }
}