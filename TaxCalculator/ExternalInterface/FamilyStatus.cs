using System.Collections.Generic;
using System.Linq;

namespace TaxCalculator.ExternalInterface
{
    public class FamilyStatus
    {
        public FamilyStatus(IEnumerable<PersonStatus> personStatuses, IEnumerable<SpendingStepInput> spendingStepInputs)
        {
            SpendingStepInputs = spendingStepInputs;
            Persons.AddRange(personStatuses);
        }

        public IEnumerable<SpendingStepInput> SpendingStepInputs { get; }
        public List<PersonStatus> Persons { get; } = new List<PersonStatus>();
        public PersonStatus PrimaryPerson => Persons.First();
    }
}