using System;
using Antigen.Util;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.Movement
{
    /// <summary>
    /// Steering behaviour that adjusts cell movement based on
    /// blood flow. Adjusts a host cell's movement vector such
    /// that the cell follows the blood flow, including some
    /// randomised variation.
    /// </summary>
    [Serializable]
    sealed class FlowSteeringBehavior : ISteeringBehavior
    {
        /// <summary>
        /// Variance of flow force strength.
        /// </summary>
        private const float StrengthVariance = 0.5f;
        /// <summary>
        /// Variance of flow force direction.
        /// </summary>
        private const int DirVariance = 80;

        /// <summary>
        /// Shared random number generator.
        /// </summary>
        private static readonly Random sRnd = new Random();

        /// <summary>
        /// The map, providing blood flow data.
        /// </summary>
        private readonly Map.Map mMap;

        /// <summary>
        /// Creates a steering behaviour for the given map.
        /// </summary>
        /// <param name="map">The map.</param>
        public FlowSteeringBehavior(Map.Map map)
        {
            mMap = map;
        }

        /// <inheritdoc />
        public void ApplySteeringForce(ICanMove host)
        {
            var pos = host.Position;
            var radius = host.Radius;

            var data = mMap.GetData(new Rectangle((int)(pos.X - radius), (int)(pos.Y - radius), radius * 2, radius * 2));

            var baseSpeed = host.MaxVelocity;
            var flowStrenght = baseSpeed * Math.Pow(data.Flow, 2) * 0.2f;
            flowStrenght *= (float)(1f + (sRnd.NextDouble() * StrengthVariance));

            var degDir = data.Dir + sRnd.Next(-DirVariance, DirVariance + 1);
            degDir %= 360;
            var flowDir = Functions.DegToRadian(degDir);

            var flowVec = Functions.PolarToCartesian(flowStrenght, flowDir);

            //Translation of origin bottom left (euclid) to top left (XNA)
            flowVec.Y *= -1;

            host.MovementVector = (host.MovementVector + flowVec).Truncate(host.MaxVelocity);
        }
    }
}
