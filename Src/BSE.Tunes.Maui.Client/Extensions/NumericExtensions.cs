﻿namespace BSE.Tunes.Maui.Client.Extensions
{
    public static class NumericExtensions
    {
        public static double Clamp(this double self, double min, double max)
        {
            if (max < min)
            {
                return max;
            }
            else if (self < min)
            {
                return min;
            }
            else if (self > max)
            {
                return max;
            }

            return self;
        }
    }
}
