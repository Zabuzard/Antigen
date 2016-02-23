using System;
using Antigen.Input;
using Antigen.Logic.Movement;
using Antigen.Logic.Offensive;
using Antigen.Logic.Offensive.Attack;
using Antigen.Logic.Offensive.Debuff;
using Antigen.Logic.Offensive.Infection;
using Antigen.Objects.Properties;
using Antigen.Objects.Units;
using Antigen.Util;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.UnitModes
{
    /// <summary>
    /// Control for offensive movement which automaticly
    /// moves the unit to a near target and attacks or debuffs it.
    /// Moves a object to right click destinations.
    /// Handles correct selections if object is selectable.
    /// </summary>
    [Serializable]
    sealed class DefensiveControl : IModeControl
    {
        /// <summary>
        /// Minimal distance to target in pixel for an attack.
        /// </summary>
        private const int MinTriggerDist = 15;
        /// <summary>
        /// Behaviour which receives the move order.
        /// </summary>
        private readonly IMoveBehavior mMovReceiver;
        /// <summary>
        /// Behaviour which receives offensive orders.
        /// </summary>
        private readonly OffensiveBehavior mOffensiveReceiver;
        /// <summary>
        /// The unit controlled by this control object.
        /// </summary>
        private readonly Unit mParent;
        /// <summary>
        /// Current target for offensive actions. <code>null</code> if no
        /// offensive action is currently being attempted.
        /// </summary>
        private Unit mOffensiveTarget;
        /// <summary>
        /// Mode of attack that should be used when attacking enemies.
        /// </summary>
        private readonly OffensiveBehavior.OffensiveMode mOffensiveMode;
        /// <summary>
        /// Whether the unit controlled by this object can attack.
        /// </summary>
        private readonly bool mCanAttack;

        /// <summary>
        /// Creates a new defensive control object which moves a object
        /// to right click destinations by ordering the receiving behaviour.
        /// Also attacks targets at right click destinations.
        /// Handles correct selections if unit is selectable.
        /// </summary>
        /// <param name="parent">Unit controlled by this object. Must
        /// implement <code>ICanMove</code> and <code>IOffensive</code></param>
        /// <param name="movReceiver">Move order receiving behaviour</param>
        /// <param name="offensiveReceiver">Attack order receiving behavior</param>
        public DefensiveControl(Unit parent, IMoveBehavior movReceiver,
            OffensiveBehavior offensiveReceiver)
        {
            mMovReceiver = movReceiver;
            mOffensiveReceiver = offensiveReceiver;
            mParent = parent;
            mCanAttack = true;

            if (parent is ICanAttack)
                mOffensiveMode = OffensiveBehavior.OffensiveMode.Attack;
            else if (parent is ICanDebuff)
                mOffensiveMode = OffensiveBehavior.OffensiveMode.Debuff;
            else if (parent is ICanDeInfect)
                mOffensiveMode = OffensiveBehavior.OffensiveMode.DeInfection;
        }

        /// <summary>
        /// Creates a new defensive control object which moves a object
        /// to right click destinations by ordering the receiving behaviour.
        /// Can not attack targets at right click destinations.
        /// Handles correct selections if unit is selectable.
        /// </summary>
        /// <param name="parent">Unit controlled by this object.</param>
        /// <param name="movReceiver">Move order receiving behaviour</param>
        public DefensiveControl(Unit parent, IMoveBehavior movReceiver)
        {
            mMovReceiver = movReceiver;
            mParent = parent;
            mCanAttack = false;
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            if (mOffensiveTarget == null)
                return;

            //Case there is a target
            var dist = mOffensiveTarget.Position.Distance(mParent.Position);
            dist -= (mOffensiveTarget.Radius + mParent.Radius);
            var closeEnough = dist <= MinTriggerDist;

            //Trigger offensive action or follow if not close enough
            if (closeEnough && !mOffensiveReceiver.IsOffensive())
            {
                var livelyTarget = mOffensiveTarget as ILively;
                if (livelyTarget != null && livelyTarget.IsAlive())
                    DoOffensiveAction(gameTime);
                else
                    mOffensiveTarget = null;
            }
        }

        /// <inheritdoc />
        public bool HandleRightClick(ClickInfo info)
        {
            //Reset target and assign a new one if clicked on enemy
            mOffensiveTarget = null;
            if (mCanAttack)
            {
                foreach (var target in info.ObjectsUnderCursor)
                {
                    if (!IsValidOffensiveTarget(target))
                        continue;

                    mOffensiveTarget = target;
                    mMovReceiver.Follow(mOffensiveTarget);
                    break;
                }
            }

            //Move to right click destination if no enemy was clicked at
            if (mOffensiveTarget == null)
                mMovReceiver.MoveTo(new Vector2(info.Location.Absolute.X, info.Location.Absolute.Y));

            return false;
        }

        /// <summary>
        /// Indicates whether <code>target</code> can be attacked by
        /// this object's parent unit.
        /// </summary>
        /// <param name="target">Another unit.</param>
        /// <returns><code>true</code> if the parent unit can attack the target;
        /// <code>false</code> otherwise.</returns>
        private bool IsValidOffensiveTarget(Unit target)
        {
            if (target == null)
                return false;

            switch (mOffensiveMode)
            {
                case OffensiveBehavior.OffensiveMode.Attack:
                    //Resharper hint: Engine should support such property combinations for future units
// ReSharper disable CSharpWarnings::CS0183
                    return target is IAttackable && Functions.IsOffensiveConstellation(mParent, target, false);
                case OffensiveBehavior.OffensiveMode.Debuff:
                    return target is IDebuffable && Functions.IsOffensiveConstellation(mParent, target, false);
                case OffensiveBehavior.OffensiveMode.DeInfection:
                    return target is ICanInfect && Functions.IsOffensiveConstellation(mParent, target, true);
// ReSharper restore CSharpWarnings::CS0183
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public void Activate()
        {   
        }

        /// <inheritdoc />
        public void Terminate()
        {
            mOffensiveTarget = null;
            mMovReceiver.Terminate();
        }

        /// <inheritdoc />
        public EventOrder EventOrder
        {
            get { return EventOrder.GameObjects; }
        }

        /// <summary>
        /// Triggers an offensive action to current target and stops movement.
        /// </summary>
        /// <param name="gameTime">GameTime object</param>
        private void DoOffensiveAction(GameTime gameTime)
        {
            if (mOffensiveTarget == null)
                return;

            mMovReceiver.Terminate();
            mOffensiveReceiver.OffensiveAction(mOffensiveTarget, gameTime);
        }
    }
}
