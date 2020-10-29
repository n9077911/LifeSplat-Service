using System;
using Calculator.Input;

namespace CalculatorTests.Utilities
{
    public static class TestPersons
    {
        public static FamilyBuilder TwoComplexPeople(DateTime now, int spending)
        {
            var person1 = GetPerson1();
            var person2 = GetPerson2();
            
            return new FamilyBuilder {Persons = new []{person1, person2}, Spending = new []{new SpendingStep(now, spending)}};
        }

        public static FamilyBuilder OnePerson(DateTime now, int spending)
        {
            return new FamilyBuilder {Persons = new []{GetPerson1()}, Spending = new []{new SpendingStep(now, spending)}};
        }
        
        private static Person GetPerson2()
        {
            return new Person {Salary = 50_000, Dob = new DateTime(1985, 12, 1), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 5m, EmployerContribution = 3m, EmergencyFundSpec = new EmergencyFundSpec("50000")};
        }

        private static Person GetPerson1()
        {
            return new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000,
                EmployeeContribution = 5m, EmployerContribution = 3m, EmergencyFundSpec = new EmergencyFundSpec("50000")};
        }

        public static FamilyBuilder TwoComplexPeople_WithPension(DateTime now, int spending, int total)
        {
            var person1 = GetPerson1();
            person1.ExistingPrivatePension = total;
            var person2 = GetPerson2();
            person2.ExistingPrivatePension = total;
            
            return new FamilyBuilder(){Persons = new []{person1, person2}, Spending = new []{new SpendingStep(now, spending)}};
        }

    }
}