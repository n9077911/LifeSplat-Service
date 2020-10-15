using System;
using System.Collections.Generic;
using System.Linq;
using Calculator.Output;

namespace Calculator.Input
{
    public class Family
    {
        public Family(IEnumerable<Person> persons, IEnumerable<SpendingStep> spendingStepInputs)
        {
            if(persons.Count(p => p.Children.Count > 0) > 1)
                throw new Exception("A Family can only have 1 child benefit claim.");
            
            SpendingStepInputs = spendingStepInputs;
            Persons.AddRange(persons);
        }
        
        public Family(Person personStatuses, IEnumerable<SpendingStep> spendingStepInputs)
        {
            SpendingStepInputs = spendingStepInputs;
            Persons.Add(personStatuses);
        }

        public IEnumerable<SpendingStep> SpendingStepInputs { get; }
        public List<Person> Persons { get; } = new List<Person>();
        public Person PrimaryPerson => Persons.First();

        public Person PersonWithChildBenefitClaim()
        {
            return Persons.FirstOrDefault(person => person.Children.Count > 0);
        }

        public Person PersonWithoutChildBenefitClaim()
        {
            return Persons.FirstOrDefault(person => person.Children.Count == 0);
        }
    }
}