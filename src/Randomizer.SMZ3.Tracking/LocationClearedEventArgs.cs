﻿using System;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Provides data for events that occur when clearing a location.
    /// </summary>
    public class LocationClearedEventArgs : TrackerEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="LocationClearedEventArgs"/> class.
        /// </summary>
        /// <param name="location">The location that was cleared.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public LocationClearedEventArgs(Location location, float? confidence)
            : base(confidence)
        {
            Location = location;
        }

        /// <summary>
        /// Gets the location that was cleared.
        /// </summary>
        public Location Location { get; }
    }
}
