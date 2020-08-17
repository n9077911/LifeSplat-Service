using System.Collections.Generic;
using System.Linq;

namespace TaxCalculator.ExternalInterface
{
    public class FamilyStatus
    {
        public FamilyStatus(IEnumerable<PersonStatus> personStatuses)
        {
            Persons.AddRange(personStatuses);
            Spending = Persons.Select(p => p.Spending).Sum();
        }

        public int Spending { get; }
        public List<PersonStatus> Persons { get; } = new List<PersonStatus>();
        public PersonStatus PrimaryPerson => Persons.First();
    }
}