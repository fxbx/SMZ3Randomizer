﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;

using Randomizer.SMZ3.Tracking.Vocabulary;
using Randomizer.SMZ3.Tracking.VoiceCommands;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Tracks items and locations in a playthrough by listening for voice
    /// commands and responding with text-to-speech.
    /// </summary>
    public class Tracker : IDisposable
    {
        private static readonly Random s_random = new();

        private readonly SpeechSynthesizer _tts;
        private readonly SpeechRecognitionEngine _recognizer;
        private readonly Action<string> _log;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tracker"/> class.
        /// </summary>
        /// <param name="log">Invoked for debug output.</param>
        /// <param name="trackerConfigProvider">
        /// Used to provide the tracking configuration.
        /// </param>
        /// <param name="world">The generated world to track in.</param>
        public Tracker(Action<string> log, TrackerConfigProvider trackerConfigProvider, World? world)
        {
            _log = log;
            _tts = new SpeechSynthesizer();
            _tts.SelectVoiceByHints(VoiceGender.Female);

            var config = trackerConfigProvider.GetTrackerConfig();
            Items = config.Items.ToImmutableList();
            Responses = config.Responses;
            World = world;

            var trackItemsCommand = new TrackItemsCommand(this);
            trackItemsCommand.ItemTracked += ItemTracked;

            _recognizer = new SpeechRecognitionEngine();
            _recognizer.LoadGrammar(trackItemsCommand.BuildGrammar());
            _recognizer.SetInputToDefaultAudioDevice();
        }

        /// <summary>
        /// Gets a collection of trackable items.
        /// </summary>
        public IReadOnlyCollection<ItemData> Items { get; }

        /// <summary>
        /// Gets the configured responses.
        /// </summary>
        public ResponseConfig Responses { get; }

        /// <summary>
        /// Gets the world for the currently tracked playthrough.
        /// </summary>
        public World? World { get; }

        /// <summary>
        /// Returns the item that has the specified name.
        /// </summary>
        /// <param name="name">The name of the item to find.</param>
        /// <returns>
        /// The <see cref="ItemData"/> with the specified name, or <c>null</c>
        /// if no items have a matching name.
        /// </returns>
        /// <remarks>
        /// Names are case insensitive, and items can be found either by their
        /// default name, additional names, or stage names.
        /// </remarks>
        public ItemData? FindItemByName(string name)
        {
            return Items.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                ?? Items.SingleOrDefault(x => x.OtherNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                ?? Items.Where(x => x.Stages != null)
                        .SingleOrDefault(x => x.Stages!.SelectMany(stage => stage.Value).Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Starts voice recognition.
        /// </summary>
        public virtual void StartTracking()
        {
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            Say(Responses.StartedTracking);
        }

        /// <summary>
        /// Stops voice recognition.
        /// </summary>
        public virtual void StopTracking()
        {
            _recognizer.RecognizeAsyncStop();
            Say(Responses.StoppedTracking, wait: true);
        }

        /// <summary>
        /// Speak a sentence using text-to-speech.
        /// </summary>
        /// <param name="phrases">The phrases to pick from.</param>
        /// <param name="args">
        /// Optional object array used to format the randomly picked phrase
        /// with.
        /// </param>
        public void Say(PhraseSet? phrases, params object?[] args)
        {
            Say(phrases, false, args);
        }

        /// <summary>
        /// Speak a sentence using text-to-speech.
        /// </summary>
        /// <param name="phrases">The phrases to pick from.</param>
        /// <param name="wait">
        /// <c>true</c> to wait until the text has been spoken completely or
        /// <c>false</c> to immediately return. The default is <c>false</c>.
        /// </param>
        /// <param name="args">
        /// Optional object array used to format the randomly picked phrase
        /// with.
        /// </param>
        public void Say(PhraseSet? phrases, bool wait, params object?[] args)
        {
            if (phrases == null || phrases.Count == 0)
                return;

            var phrase = phrases.Random(s_random);
            var text = string.Format(phrase, args);
            Say(text, wait);
        }

        /// <summary>
        /// Speak a sentence using text-to-speech.
        /// </summary>
        /// <param name="text">The phrase to speak.</param>
        /// <param name="wait">
        /// <c>true</c> to wait until the text has been spoken completely or
        /// <c>false</c> to immediately return. The default is <c>false</c>.
        /// </param>
        public virtual void Say(string text, bool wait = false)
        {
            if (wait)
                _tts.Speak(text);
            else
                _tts.SpeakAsync(text);
        }

        /// <summary>
        /// Cleans up resources used by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleans up resources used by this class.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to dispose of managed resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _recognizer.Dispose();
                    _tts.Dispose();
                }

                _disposed = true;
            }
        }

        private void ItemTracked(object? sender, ItemTrackedEventArgs e)
        {
            _log($"Recognized item {e.Item.Name} as {e.TrackedAs} with {e.Confidence:P2} confidence.");

            var accessibleBefore = GetAccessibleLocations();
            var itemName = e.Item.GetRandomName(s_random);

            if (e.Item.HasStages)
            {
                var stage = e.Item.GetStage(e.TrackedAs);
                if (stage != null)
                {
                    // Tracked by specific stage name (e.g. Tempered Sword), set
                    // to that stage specifically
                    var stageName = e.Item.Stages[stage.Value].Random(s_random);
                    if (e.Item.Track(stage.Value))
                        Say(Responses.TrackedItemByStage, itemName, stageName);
                    else
                        Say(Responses.TrackedOlderProgressiveItem, itemName, e.Item.Stages[e.Item.TrackingState].Random(s_random));
                }
                else
                {
                    // Tracked by regular name, upgrade by one step
                    if (e.Item.Track())
                    {
                        // Say($"Upgraded {randomName} by one step.");
                        var stageName = e.Item.Stages[e.Item.TrackingState].Random(s_random);
                        Say(Responses.TrackedProgressiveItem, itemName, stageName);
                    }
                    else
                    {
                        Say(Responses.TrackedTooManyOfAnItem);
                    }
                }
            }
            else if (e.Item.Multiple)
            {
                e.Item.Track();
                Say(Responses.TrackedItemMultiple, itemName);
            }
            else
            {
                if (e.Item.Track())
                    Say(Responses.TrackedItem, e.Item.Name);
                else
                    Say(Responses.TrackedAlreadyTrackedItem);
            }

            GiveLocationHint(accessibleBefore);
        }

        private void GiveLocationHint(IEnumerable<Location> accessibleBefore)
        {
            if (World == null)
                return;

            var accessibleAfter = GetAccessibleLocations();
            var newlyAccessible = accessibleAfter.Except(accessibleBefore);
            if (newlyAccessible.Any())
            {
                var regions = newlyAccessible.GroupBy(x => x.Region)
                    .OrderByDescending(x => x.Count())
                    .ThenBy(x => x.Key.Name);
                foreach (var region in regions)
                    _log($"{region.Count()} location(s) in {region.Key.Name}: {string.Join(", ", region.Select(x => x.Name))}");

                if (newlyAccessible.Contains(World.InnerMaridia.ShaktoolItem))
                    Say(Responses.ShaktoolAvailable);

                if (newlyAccessible.Contains(World.DarkWorldNorthWest.PegWorld))
                    Say(Responses.PegWorldAvailable);
            }
            else
            {
                var rnd = s_random.Next();
                if (rnd % 20 == 0)
                    Say("Doesn't get you anywhere though.");
            }
        }

        private IEnumerable<Location> GetAccessibleLocations()
        {
            if (World == null)
                return Enumerable.Empty<Location>();

            var items = new List<Item>();
            foreach (var item in Items)
            {
                for (var i = 0; i < item.TrackingState; i++)
                    items.Add(new Item(item.InternalItemType));
            }

            var progression = new Progression(items);
            return World.Locations.Where(x => x.IsAvailable(progression)).ToList();
        }
    }
}
