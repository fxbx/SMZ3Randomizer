﻿using System.Collections.Generic;
using System.Linq;

namespace Randomizer.SMZ3
{
    /// <summary>
    /// Represents a region in a game.
    /// </summary>
    public abstract class Region
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Region"/> class for the
        /// specified world and configuration.
        /// </summary>
        /// <param name="world">The world the region is in.</param>
        /// <param name="config">The config used.</param>
        protected Region(World world, Config config)
        {
            Config = config;
            World = world;
        }

        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the name of the overall area the region is a part of.
        /// </summary>
        public virtual string Area => Name;

        /// <summary>
        /// Gets a collection of alternate names for the region.
        /// </summary>
        public virtual List<string> AlsoKnownAs { get; } = new();

        /// <summary>
        /// Gets or sets a collection of every location in the region.
        /// </summary>
        public IEnumerable<Location> Locations => GetLocations()
            .Concat(GetRooms().SelectMany(x => x.GetLocations()));

        /// <summary>
        /// Gets the world the region is located in.
        /// </summary>
        public World World { get; }

        /// <summary>
        /// Gets the relative weight used to bias the randomization process.
        /// </summary>
        public int Weight { get; init; } = 0;

        /// <summary>
        /// Gets the randomizer configuration options.
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// Gets the list of region-specific items, e.g. keys, maps, compasses.
        /// </summary>
        protected IList<ItemType> RegionItems { get; set; } = new List<ItemType>();

        /// <summary>
        /// Determines whether the specified item is specific to this region.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>
        /// <see langword="true"/> if the item is specific to this region;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsRegionItem(Item item)
        {
            return RegionItems.Contains(item.Type);
        }

        /// <summary>
        /// Determines whether the specified item can be assigned to a location
        /// in this region.
        /// </summary>
        /// <param name="item">The item to fill.</param>
        /// <param name="items">The currently available items.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="item"/> can be
        /// assigned to a location in this region; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        public virtual bool CanFill(Item item, Progression items)
        {
            return Config.Keysanity || !item.IsDungeonItem || IsRegionItem(item);
        }

        /// <summary>
        /// Returns a string that represents the region.
        /// </summary>
        /// <returns>A new string that represents the region.</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Determines whether the region can be entered with the specified
        /// items.
        /// </summary>
        /// <param name="items">The currently available items.</param>
        /// <returns>
        /// <see langword="true"/> if the region is available with <paramref
        /// name="items"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool CanEnter(Progression items)
        {
            return true;
        }

        protected IEnumerable<Location> GetLocations()
            => GetType().GetPropertyValues<Location>(this);

        protected IEnumerable<Room> GetRooms()
            => GetType().GetPropertyValues<Room>(this);
    }
}
