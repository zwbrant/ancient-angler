using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class MathUtilities
{
    /// <summary>
    /// Fraction of difference between floats and the largest float.
    /// </summary>
    /// <param name="comparisonFloat">float to compare to</param>
    /// <returns></returns>
    public static float RatioOfExcess(this float f, float comparisonFloat)
    {
        float absDifference = Math.Abs(f - comparisonFloat);
        return (f > comparisonFloat) ? absDifference / f : absDifference / comparisonFloat;
    }
}

