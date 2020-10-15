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
            var take25LumpSum = pensionPot * .25m;
            var take25Cap = _lifeTimeAllowance * .25m;
            take25LumpSum = Math.Min(take25LumpSum, take25Cap);

            var newPensionPot = pensionPot - take25LumpSum;

            return new Take25Result(newPensionPot, take25LumpSum);
        }
    }
}