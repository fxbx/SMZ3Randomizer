﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Randomizer.SMZ3.ChatIntegration.Models;

namespace Randomizer.SMZ3.Twitch.Models
{
    public class TwitchPoll : TwitchAPIResponse
    {
        [JsonPropertyName("broadcaster_id")]
        public string? BroadcasterId { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("choices")]
        public ICollection<TwitchPollChoice>? Choices { get; set; }

        [JsonPropertyName("duration")]
        public int? Duration { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// If the poll was finished, be it complete or closed
        /// </summary>
        public bool IsPollComplete => !"ACTIVE".Equals(Status, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// If a poll was finished successfully
        /// </summary>
        public bool IsPollSuccessful => WinningChoice != null && "COMPLETED".Equals(Status, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Returns choice that was voted on the most
        /// </summary>
        public TwitchPollChoice? WinningChoice => Choices?.OrderByDescending(x => x.Votes).FirstOrDefault();
    }
}
