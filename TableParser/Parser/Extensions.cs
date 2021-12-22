using System;
using Google.Apis.Sheets.v4.Data;

namespace TableParser.Parser
{
    public static class ColorExtansions
    {
        // TODO: Возможность исопльзовать оттенки
        public static bool EqualTo(this Color color, Color other)
        {
            return color.Red.EqualToWithPrecision(other.Red) 
                && color.Green.EqualToWithPrecision(other.Green) 
                && color.Blue.EqualToWithPrecision(other.Blue);
        }
    }

    public static class FloatExtensions
    {
        public static bool EqualToWithPrecision(this float? num, float? other)
        {
            if (num != null && other != null)
                return Math.Abs((float)num - (float)other) <= 1e-4;

            throw new Exception("Float is null");
        }
    }
}
