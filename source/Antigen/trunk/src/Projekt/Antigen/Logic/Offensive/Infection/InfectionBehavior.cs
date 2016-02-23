using System;
using Antigen.Objects;
using Antigen.Objects.Properties;
using Antigen.Objects.Units;
using Antigen.Util;

namespace Antigen.Logic.Offensive.Infection
{
    /// <summary>
    /// Implements a basic infection behavior for units.
    /// Can infect untis with another.
    /// </summary>
    [Serializable]
    sealed class InfectionBehavior : IInfectionBehavior
    {
        /// <summary>
        /// Object to infect with.
        /// </summary>
        private readonly ICanInfect mObj;

        private IInfectable mTarget;

        /// <summary>
        /// Returns if the object is currently infecting other objects.
        /// </summary>
        public bool IsInfecting { get; private set; }

        /// <summary>
        /// Creates a new basic infection behavior with a given object.
        /// </summary>
        /// <param name="obj">Object to infect with</param>
        public InfectionBehavior(ICanInfect obj)
        {
            mObj = obj;
            IsInfecting = false;
        }

        /// <inheritdoc />
        public void Infect(IInfectable target)
        {
            if (!Functions.IsOffensiveConstellation((Unit) mObj, (Unit) target, false))
            {
                return;
            }

            if (target.GetInfectionResistance() >= mObj.GetInfectionPower())
            {
                return;
            }

            IsInfecting = true;
            mTarget = target;
            mObj.HasInfected();
            target.GettingInfected();
            
            //Destroy target at infection and beam object to targets position
            var lively = mTarget as ILively;
            //Resharper hint: Engine should support such property combinations
// ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (lively != null)
// ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                lively.ChangeLifepoints(-lively.GetLifepoints());
            }
            var gameObj = mObj as GameObject;
            var gameTarget = mTarget as GameObject;
            //Resharper hint: Engine should support such property combinations
// ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (gameObj != null && gameTarget != null)
// ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                gameObj.Position = gameTarget.Position;
            }
        }

        /// <summary>
        /// Gets the infected target or null if currently not infecting.
        /// Target unit was destroyed during infection and removed from all lists.
        /// </summary>
        /// <returns>Infected target or null if currently not infecting</returns>
        public IInfectable GetTarget()
        {
            return mTarget;
        }

        /// <summary>
        /// Cuts the connection between infecter and infected.
        /// Destroys the infected object.
        /// </summary>
        public void CutConnection()
        {
            if (!IsInfecting || mTarget == null)
            {
                return;
            }

            var lively = mTarget as ILively;
            if (lively != null)
            {
                lively.ChangeLifepoints(-lively.GetLifepoints());
            }
            mTarget = null;
        }
    }
}
