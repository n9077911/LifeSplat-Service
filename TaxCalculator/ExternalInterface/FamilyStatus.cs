using System.Collections.Generic;
using System.Linq;

namespace TaxCalculator.ExternalInterface
{
    public class FamilyStatus
    {
        public FamilyStatus(IEnumerable<PersonStatus> personStatuses)
        {
            Persons.AddRange(personStatuses);
            Spending = PrimaryPerson.Spending; //TODO: refactor spending out of person status? or perhaps family should be able to specify individual spending?
        }

        public int Spending { get; }
        public List<PersonStatus> Persons { get; } = new List<PersonStatus>();
        public PersonStatus PrimaryPerson => Persons.First();

        //TODO: refactor to the idea of FamilyOverTime
        public void BalanceSavings()
        {
            var calcMinSavings = Persons.Select(p => p.CalcMinimumSteps.CurrentStep.Savings).Sum();
            var targetSavings = Persons.Select(p => p.TargetSteps.CurrentStep.Savings).Sum();
            
            foreach (var person in Persons)
            {
                person.CalcMinimumSteps.SetSavings(calcMinSavings / Persons.Count);
                person.TargetSteps.SetSavings(targetSavings / Persons.Count);
            }
        }
    }
}