﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

using Randomizer.SMZ3;

namespace Randomizer.App.ViewModels
{
    public class RandomizerOptions : INotifyPropertyChanged
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public RandomizerOptions()
        {
            GeneralOptions = new GeneralOptions();
            SeedOptions = new SeedOptions();
            PatchOptions = new PatchOptions();
        }

        [JsonConstructor]
        public RandomizerOptions(GeneralOptions generalOptions,
            SeedOptions seedOptions,
            PatchOptions patchOptions)
        {
            GeneralOptions = generalOptions ?? new();
            SeedOptions = seedOptions ?? new();
            PatchOptions = patchOptions ?? new();
        }

        [JsonPropertyName("General")]
        public GeneralOptions GeneralOptions { get; }

        [JsonPropertyName("Seed")]
        public SeedOptions SeedOptions { get; }

        [JsonPropertyName("Patch")]
        public PatchOptions PatchOptions { get; }

        public bool ItemLocationsExpanded { get; set; } = false;

        public bool CustomizationExpanded { get; set; } = false;

        public bool CommonExpanded { get; set; } = true;

        public double WindowWidth { get; set; } = 500d;

        public double WindowHeight { get; set; } = 600d;

        public static RandomizerOptions Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<RandomizerOptions>(json, s_jsonOptions);
        }

        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(this, s_jsonOptions);
            File.WriteAllText(path, json);
        }

        public Config ToConfig() => new()
        {
            GameMode = GameMode.Normal,
            Z3Logic = Z3Logic.Normal,
            SMLogic = SMLogic.Normal,
            SwordLocation = SeedOptions.SwordLocation,
            MorphLocation = SeedOptions.MorphLocation,
            MorphBombsLocation = SeedOptions.MorphBombsLocation,
            ShaktoolItemPool = SeedOptions.ShaktoolItem,
            PegWorldItemPool = SeedOptions.PegWorldItem,
            KeyShuffle = SeedOptions.Keysanity ? KeyShuffle.Keysanity : KeyShuffle.None,
            Race = SeedOptions.Race,
            ExtendedMsuSupport = PatchOptions.CanEnableExtendedSoundtrack && PatchOptions.EnableExtendedSoundtrack,
            ShuffleDungeonMusic = PatchOptions.ShuffleDungeonMusic,
            HeartColor = PatchOptions.HeartColor,
            LowHealthBeepSpeed = PatchOptions.LowHealthBeepSpeed,
            DisableLowEnergyBeep = PatchOptions.DisableLowEnergyBeep
        };

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }
    }
}