using System;
using Calculator.Input;

namespace CalculatorTests.Utilities
{
    public class TestPersons
    {
        public static FamilyBuilder TwoComplexPeople(DateTime now, int spending)
        {
            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000,
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1985, 12, 1), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};
            
            return new FamilyBuilder(){Persons = new []{person1, person2}, Spending = new []{new SpendingStep(now, spending)}};
        }
    }
}