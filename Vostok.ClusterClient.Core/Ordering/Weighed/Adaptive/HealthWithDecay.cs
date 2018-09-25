﻿using System;

namespace Vostok.ClusterClient.Core.Ordering.Weighed.Adaptive
{
    /// <summary>
    /// Represents a replica health with decay
    /// </summary>
    public struct HealthWithDecay
    {
        /// <summary>
        /// Health value
        /// </summary>
        public readonly double Value;
        
        /// <summary>
        /// Decay pivot
        /// </summary>
        public readonly DateTime DecayPivot;

        /// <summary>
        /// Initializes a new instance of <see cref="HealthWithDecay"/> class.
        /// </summary>
        public HealthWithDecay(double value, DateTime decayPivot)
        {
            Value = value;
            DecayPivot = decayPivot;
        }
    }
}