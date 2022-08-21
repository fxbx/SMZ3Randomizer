﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Service for getting layouts and images for displaying in the UI
    /// </summary>
    public interface IUIService
    {
        /// <summary>
        /// Retrieves a list of layouts that can be selected by the user
        /// </summary>
        public List<UILayout> SelectableLayouts { get; }

        /// <summary>
        /// Retrieves a layout by name
        /// </summary>
        /// <param name="name">The name of the requested</param>
        /// <returns>The matching layout or the first one if it is not found</returns>
        public UILayout GetLayout(string name);

        /// <summary>
        /// Returns the path of the sprite for the number
        /// </summary>
        /// <param name="digit">The number requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(int digit);

        /// <summary>
        /// Returns the path of the sprite for the item
        /// </summary>
        /// <param name="item">The item requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(ItemData item);

        /// <summary>
        /// Returns the path of the sprite for the boss
        /// </summary>
        /// <param name="boss">The boss requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(BossInfo boss);

        /// <summary>
        /// Returns the path of the sprite for the dungeon
        /// </summary>
        /// <param name="dungeon">The dungeon requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(DungeonInfo dungeon);

        /// <summary>
        /// Returns the path of the sprite for the reward
        /// </summary>
        /// <param name="reward">The reward requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(RewardItem reward);

        /// <summary>
        /// Returns the path of the sprite
        /// </summary>
        /// <param name="category">The category of sprite</param>
        /// <param name="imageFileName">The individual filename of the sprite</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(string category, string imageFileName);
    }
}
