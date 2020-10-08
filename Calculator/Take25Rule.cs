using System;

namespace Calculator
{
    public class Take25Rule
    {
        private readonly int _lifeTimeAllowance;

        public Take25Rule(in int lifeTimeAllowance)
        {
            _lifeTimeAllowance = lifeTimeAllowance;
        }

        public Take25Result Result(decimal pensionPot)
        {
            var originalPensionPot = pensionPot;
            var amountSubjectToLta = Math.Max(0, pensionPot - _lifeTimeAllowance);
            var ltaCharge = amountSubjectToLta * .25m;
            pensionPot -= ltaCharge;
            
            var take25LumpSum = pensionPot * .25m;
            var take25Cap = _lifeTimeAllowance * .25m;
            take25LumpSum = Math.Min(take25LumpSum, take25Cap);

            var newPensionPot = pensionPot - take25LumpSum;

            return new Take25Result(originalPensionPot, newPensionPot, take25LumpSum, ltaCharge);
        }
    }
}