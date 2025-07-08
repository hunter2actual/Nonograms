using System;
using System.Collections.Generic;

namespace Nonograms.Game;

public static class Array2dExtensions 
{
    public static List<T> ToList<T>(this T[,] array)
    {
        var result = new List<T>();
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                result.Add(array[i, j]);
            }
        }
        return result;
    }
    
    public static TOut[,] Select2D<TIn, TOut>(this TIn[,] array, Func<TIn, TOut> selector)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);
        var result = new TOut[rows, cols];

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                result[i, j] = selector(array[i, j]);
            }
        }

        return result;
    }
}