﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

using Randomizer.SMZ3.Tracking.Vocabulary;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Represents a trackable item.
    /// </summary>
    public class ItemData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemData"/> class with
        /// the specified item name and type.
        /// </summary>
        /// <param name="name">The possible names of the item.</param>
        /// <param name="internalItemType">
        /// The internal <see cref="ItemType"/> of the item.
        /// </param>
        public ItemData(SchrodingersString name, ItemType internalItemType)
        {
            Name = name;
            InternalItemType = internalItemType;
        }

        /// <summary>
        /// Gets the name for the item.
        /// </summary>
        public SchrodingersString Name { get; init; }

        /// <summary>
        /// Gets the internal <see cref="ItemType"/> of the item.
        /// </summary>
        public ItemType InternalItemType { get; init; }

        /// <summary>
        /// Indicates whether the item can be tracked more than once.
        /// </summary>
        public bool Multiple { get; init; }

        /// <summary>
        /// Gets the stages and their names of a progressive item.
        /// </summary>
        public IReadOnlyDictionary<int, SchrodingersString>? Stages { get; init; }

        /// <summary>
        /// Gets or sets the zero-based index of the column in which the item
        /// should be displayed on the tracker.
        /// </summary>
        public int? Column { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index of the column in which the item
        /// should be displayed on the tracker.
        /// </summary>
        public int? Row { get; set; }

        /// <summary>
        /// Gets or sets the path to the image to be displayed on the tracker.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Gets the highest stage the item supports, or 0 if the item does not
        /// have stages.
        /// </summary>
        public int MaxStage => HasStages ? Stages.Max(x => x.Key) : default;

        /// <summary>
        /// Indicates whether the item has stages.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Stages))]
        public bool HasStages => Stages != null && Stages.Count > 0;

        /// <summary>
        /// Gets the tracking state of the item.
        /// </summary>
        /// <remarks>
        /// For example, 0 represents an untracked item, 1 represents an
        /// obtained item and higher values indicate items that have been
        /// obtained more than once.
        /// </remarks>
        [JsonIgnore]
        public int TrackingState { get; protected set; }

        /// <summary>
        /// Returns the stage of the item with the specifies name.
        /// </summary>
        /// <param name="name">The name of the stage.</param>
        /// <returns>
        /// The number of the stage with the given name, or <c>null</c> if the
        /// name does not match a stage.
        /// </returns>
        public int? GetStage(string name)
        {
            if (Stages?.Any(x => x.Value.Contains(name, StringComparison.OrdinalIgnoreCase)) == true)
                return Stages.Single(x => x.Value.Contains(name, StringComparison.OrdinalIgnoreCase)).Key;

            return null;
        }

        /// <summary>
        /// Tracks the item.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the item was tracked; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Tracking may fail if the item is already tracked, or if the item is
        /// already at the highest stage.
        /// </remarks>
        public bool Track()
        {
            if (TrackingState == 0 // Item hasn't been tracked yet (any case)
                || (!HasStages && Multiple) // Multiple items always track
                || (HasStages && TrackingState < MaxStage)) // Hasn't reached max. stage yet
            {
                TrackingState++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Marks the item at the specified stage.
        /// </summary>
        /// <param name="stage">The stage to set the item to.</param>
        /// <returns>
        /// <see langword="true"/> if the item was tracked; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Tracking may fail if the item is already at a higher stage.
        /// </remarks>
        public bool Track(int stage)
        {
            if (!HasStages)
                throw new ArgumentException($"The item '{Name}' does not have multiple stages.");

            if (stage > MaxStage)
                throw new ArgumentOutOfRangeException($"Cannot advance item '{Name}' to stage {stage} as the highest state is {MaxStage}.");

            if (TrackingState < stage)
            {
                TrackingState = stage;
                return true;
            }

            return false;
        }
    }
}