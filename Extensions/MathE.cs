using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterLanis
{
    class MathE
    {
        public static T Clamp<T>(T value, T min, T max) where T : System.IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }

        public static bool InRange<T>(T value, T min, T max) where T : System.IComparable<T>
        {
            if (value.CompareTo(max) < 0 && 
                value.CompareTo(min) > 0)
                return true;
            return false;
        }

        public static T[,] MoveRowToRight<T>(T[,] value, int rowIndex, int startIndex)
        {
            for (int i = value.GetLength(0) - 1; i > startIndex; i--)
            {
                if (i != 0)
                {
                    value[i, rowIndex] = value[i - 1, rowIndex];
                    value[i - 1, rowIndex] = default;
                }
                else
                    value[i, rowIndex] = default;
            }

            return value;
        }
    }
}