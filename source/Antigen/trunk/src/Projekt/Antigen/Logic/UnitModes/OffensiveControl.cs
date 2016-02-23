using System;
using System.Collections.Generic;
using System.Linq;
using Antigen.Input;
using Antigen.Logic.Collision;
using Antigen.Logic.Movement;
using Antigen.Logic.Offensive;
using Antigen.Logic.Offensive.Attack;
using Antigen.Logic.Offensive.Debuff;
using Antigen.Logic.Offensive.Infection;
using Antigen.Objects.Properties;
using Antigen.Objects.Units;
using Antigen.Objects.Units.Values;
using Antigen.Util;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.UnitModes
{
    /// <summary>
    /// Control for offensive movement which automaticly
    /// moves the unit to a near target and attacks, debuffs or deinfects it.
    /// Moves a object to right click destinations.
    /// Handles correct selections if object is selectable.
    /// </summary>
    [Serializable]
    sealed class OffensiveControl : IModeControl
    {
        /// <summary>
        /// Minimal distance to target in pixel for an attack.
        /// </summary>
        private const int MinTriggerDist = 15;
        /// <summary>
        /// Radius in pixel for which the unit acts offensive and automaticly
        /// attacks targets if no sight value is avaiable.
        /// </summary>
        private const float AlternateSearchRadius = ValueStore.MaxSight / 2;
        /// <summary>
        /// Behaviour which receives the move order.
        /// </summary>
        private readonly IMoveBehavior mMovReceiver;
        /// <summary>
        /// Behaviour which receives offensive orders.
        /// </summary>
        private readonly OffensiveBehavior mOffensiveReceiver;
        /// <summary>
        /// Object to do offensive actions with that also must be moveable
        /// </summary>
        private readonly Unit mParent;
        /// <summary>
        /// Current target obj to be offensive towards. Null if no such
        /// object exists.
        /// </summary>
        private Unit mOffensiveTarget;
        /// <summary>
        /// Units grid.
        /// </summary>
        private readonly ISpatialCache mSpatialCache;
        /// <summary>
        /// Mode of attack that should be used when attacking
        /// enemy units.
        /// </summary>
        private readonly OffensiveBehavior.OffensiveMode mOffensiveMode;

        /// <summary>
        /// Creates a new offensive movement control object which moves a object
        /// to right click destinations by ordering the receiving behaviour.
        /// Automaticly moves units to near targets and attacks, debuffs or deinfects them.
        /// Handles correct selections if unit is selectable.
        /// </summary>
        /// <param name="parent">Unit controlled by this object. Must implement <see cref="ICanOffensive"/>
        /// and <see cref="IHasSpeed"/></param>
        /// <param name="movReceiver">Move order receiving behaviour</param>
        /// <param name="offensiveReceiver">Attack order receiving behavior</param>
        /// <param name="thatSpatialCache">Cache for spatial queries.</param>
        public OffensiveControl(Unit parent, IMoveBehavior movReceiver,
            OffensiveBehavior offensiveReceiver, ISpatialCache thatSpatialCache)
        {
            mMovReceiver = movReceiver;
            mOffensiveReceiver = offensiveReceiver;
            mParent = parent;
            mSpatialCache = thatSpatialCache;

            if (mParent is ICanAttack)
                mOffensiveMode = OffensiveBehavior.OffensiveMode.Attack;
            else if (mParent is ICanDebuff)
                mOffensiveMode = OffensiveBehavior.OffensiveMode.Debuff;
            else if (mParent is ICanDeInfect)
                mOffensiveMode = OffensiveBehavior.OffensiveMode.DeInfection;
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            //Case 1: Object is not attacking or hunting another object.
            if (mOffensiveTarget == null && !mMovReceiver.IsMoving)
            {
                //If object is currently not moving, then object will search for enemies
                var list = mSpatialCache.UnitsInArea(SearchArea()).ToList();
                list.Remove(mParent);

                var closestTarget = ClosestTarget(list);
                if (closestTarget != null)
                {
                    mOffensiveTarget = closestTarget;
                    mMovReceiver.Follow(closestTarget);
                }
            }

            //Case 2: Object is trying to attack another object.
            if (mOffensiveTarget != null)
            {
                var dist = mOffensiveTarget.Position.Distance(mParent.Position);
                dist -= (mOffensiveTarget.Radius + mParent.Radius);
                var closeEnough = dist <= MinTriggerDist;

                //Trigger offensive action or continue following if not close enough
                if (closeEnough && !mOffensiveReceiver.IsOffensive())
                {
                    var livelyTarget = mOffensiveTarget as ILively;
                    if (livelyTarget != null && livelyTarget.IsAlive())
                        DoOffensiveAction(gameTime);
                    else
                    {
                        mOffensiveTarget = null;
                        mMovReceiver.Terminate();
                    }  
                }
            }
        }

        /// <summary>
        /// Area that the unit will search for enemies in. If the unit is currently passive
        /// and an enemy enters this area, the unit will attack it.
        /// </summary>
        /// <returns>The unit's search area.</returns>
        private Rectangle SearchArea()
        {
            var searchRadius = AlternateSearchRadius;
            var hasSight = mParent as IHasSight;
            if (hasSight != null)
                searchRadius = ValueStore.ConvertSightToPixel(hasSight.GetSight());

            return new Rectangle(
                (int)(mParent.Position.X - searchRadius),
                (int)(mParent.Position.Y - searchRadius),
                (int)(searchRadius * 2),
                (int)(searchRadius * 2));
        }

        /// <summary>
        /// The element of <code>units</code> that is closest to the
        /// parent unit while also being a valid attack target.
        /// </summary>
        /// <param name="units">Any collection of units.</param>
        /// <returns>The closest attack target, or <code>null</code>
        /// if no element of <code>units</code> can be attacked by
        /// the parent unit.</returns>
        private Unit ClosestTarget(IEnumerable<Unit> units)
        {
            Unit closestTarget = null;
            var minDist = double.MaxValue;
            foreach (var target in units.Where(IsValidOffensiveTarget))
            {
                var dist = mParent.Position.Distance(target.Position);
                if (dist >= minDist)
                    continue;

                minDist = dist;
                closestTarget = target;
            }
            return closestTarget;
        }

        /// <summary>
        /// Indicates whether the given <code>target</code> can be
        /// attacked by the parent unit.
        /// </summary>
        /// <param name="target">Any unit.</param>
        /// <returns><code>true</code> if the parent unit can
        /// attack <code>target</code>; <code>false</code>
        /// otherwise.</returns>
        private bool IsValidOffensiveTarget(Unit target)
        {
            var infectableTarget = target as IInfectable;
            if (infectableTarget != null && infectableTarget.IsInfected())
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
                default:
                    return false;
// ReSharper restore CSharpWarnings::CS0183
            }
        }

        /// <inheritdoc />
        public bool HandleRightClick(ClickInfo info)
        {
            //Reset target and assign a new one if clicked on enemy
            mOffensiveTarget = null;
            foreach (var target in info.ObjectsUnderCursor.Where(IsValidOffensiveTarget))
            {
                mOffensiveTarget = target;
                break;
            }

            //Move to right click destination if no enemy was clicked at
            if (mOffensiveTarget != null)
                return false;

            mMovReceiver.MoveTo(new Vector2(info.Location.Absolute.X, info.Location.Absolute.Y));
            return false;
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
