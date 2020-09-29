using System.Collections.Generic;
using System.Linq;

namespace Calculator.Input
{
    public class Family
    {
        public Family(IEnumerable<Person> personStatuses, IEnumerable<SpendingStep> spendingStepInputs)
        {
            SpendingStepInputs = spendingStepInputs;
            Persons.AddRange(personStatuses);
        }
        
        public Family(Person personStatuses, IEnumerable<SpendingStep> spendingStepInputs)
        {
            SpendingStepInputs = spendingStepInputs;
            Persons.Add(personStatuses);
        }

        public IEnumerable<SpendingStep> SpendingStepInputs { get; }
        public List<Person> Persons { get; } = new List<Person>();
        public Person PrimaryPerson => Persons.First();
    }
}