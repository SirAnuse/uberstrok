﻿namespace UberStrok.Core.Common
{
    public static class MathUtils
    {
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;

            return value;
        }
    }
}
