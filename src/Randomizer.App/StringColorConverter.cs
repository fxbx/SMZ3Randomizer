﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Randomizer.App
{
    [ValueConversion(typeof(Color), typeof(string))]
    internal class StringColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new ArgumentException($"Invalid target type '{targetType}', expected string.");

            var color = (Color)value;
            if (color.A == 0xFF)
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";

            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Color))
                throw new ArgumentException($"Invalid target type '{targetType}', expected Color.");

            var hex = ((string)value).AsSpan();
            if (hex.Length < 1)
                return null;

            if (hex[0] != '#')
                return new ValidationResult("Expected color in the format #RGB, #RRGGBB or #AARRGGBB");

            if (!TryParseColor(hex, out var a, out var r, out var g, out var b))
                return new ValidationResult("Expected color in the format #RGB, #RRGGBB or #AARRGGBB");

            return Color.FromArgb(a, r, g, b);
        }

        private static bool TryParseColor(ReadOnlySpan<char> value,
            out byte a, out byte r, out byte g, out byte b)
        {
            const NumberStyles style = NumberStyles.HexNumber;
            var format = CultureInfo.InvariantCulture;

            a = 0;
            r = 0;
            g = 0;
            b = 0;

            if (value.Length == 4)
            {
                a = 0xFF;
                if (byte.TryParse(value.Slice(1, 1), style, format, out r)
                    && byte.TryParse(value.Slice(2, 1), style, format, out g)
                    && byte.TryParse(value.Slice(3, 1), style, format, out b))
                {
                    r *= 0x11;
                    g *= 0x11;
                    b *= 0x11;
                    return true;
                }
            }
            else if (value.Length == 7)
            {
                a = 0xFF;
                return byte.TryParse(value.Slice(1, 2), style, format, out r)
                    && byte.TryParse(value.Slice(3, 2), style, format, out g)
                    && byte.TryParse(value.Slice(5, 2), style, format, out b);
            }
            else if (value.Length == 9)
            {
                return byte.TryParse(value.Slice(1, 2), style, format, out a)
                    && byte.TryParse(value.Slice(3, 2), style, format, out r)
                    && byte.TryParse(value.Slice(5, 2), style, format, out g)
                    && byte.TryParse(value.Slice(7, 2), style, format, out b);
            }

            return false;
        }
    }
}
