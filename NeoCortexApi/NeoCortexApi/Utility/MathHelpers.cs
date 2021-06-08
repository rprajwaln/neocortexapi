﻿// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NeoCortexApi.Utility
{
    public class MathHelpers
    {
        /// <summary>
        /// Calculates the hamming distance between arrays.
        /// </summary>
        /// <param name="originArray">Original array to compare from.</param>
        /// <param name="comparingArray">Array to compare to.</param>
        /// <returns>Hamming distance.</returns>
        public static double GetHammingDistance(int[] originArray, int[] comparingArray, bool countNoneZerosOnly = false)
        {

            double[] arr1 = ArrayUtils.ToDoubleArray(originArray);
            double[] arr2 = ArrayUtils.ToDoubleArray(comparingArray);
            return GetHammingDistance(new double[][] { arr1 }, new double[][] { arr2 }, countNoneZerosOnly)[0];
        }

        /// <summary>
        /// Calculates the hamming distance between arrays.
        /// </summary>
        /// <param name="originArray">Original array to compare from.</param>
        /// <param name="comparingArray">Array to compare to.</param>
        /// <returns>Hamming distance.</returns>
        public static double[] GetHammingDistance(double[][] originArray, double[][] comparingArray, bool countNoneZerosOnly = false)
        {
            double[][] hDistance = new double[originArray.Length][];
            double[] h = new double[originArray.Length];
            double[] hammingDistance = new double[originArray.Length];

            for (int i = 0; i < originArray.Length; i++)
            {
                int len = Math.Max(originArray[i].Length, comparingArray[i].Length);
                int numOfDifferentBits = 0;
                for (int j = 0; j < len; j++)
                {
                    if (originArray[i].Length > j && comparingArray[i].Length > j)
                    {
                        if (originArray[i][j] == comparingArray[i][j])
                        {
                            numOfDifferentBits = numOfDifferentBits + 0; //TODO meaning of this operation
                        }
                        else
                        {
                            if (countNoneZerosOnly == false)
                                numOfDifferentBits++;
                            else
                            {
                                if (originArray[i][j] == 1)
                                    numOfDifferentBits++;
                            }
                        }
                    }
                    else
                        numOfDifferentBits++;
                }

                h[i] = numOfDifferentBits;
                if (originArray[i].Length > 0 && originArray[i].Count(b => b == 1) > 0)
                {
                    //hammingDistance[i] = ((originArray[i].Length - numOfDifferentBits) * 100 / originArray[i].Length);
                    if (countNoneZerosOnly == true)
                    {
                        hammingDistance[i] = ((originArray[i].Count(b => b == 1) - numOfDifferentBits) * 100 / originArray[i].Count(b => b == 1));
                    }
                    else
                    {
                        hammingDistance[i] = ((originArray[i].Length - numOfDifferentBits) * 100 / originArray[i].Length);
                    }

                }
                else
                    hammingDistance[i] = double.NegativeInfinity;
            }

            return hammingDistance;
        }

        public static bool Match(int[] originArray, int[] comparingArray, float thresholdPct)
        {
            var res = GetHammingDistance(originArray, comparingArray, true);
            return true;

        }


        /// <summary>
        /// Calculates how many elements of the array are same in percents. This method is useful to compare 
        /// two arays that contains indicies of active column
        /// <param name="originArray">Indexes of non-zero bits in the SDR.</param>
        /// <param name="comparingArray">Indexes of non-zero bits in the SDR.</param>
        /// <returns>SiCalcArraySimilarity(int[] originArray, int[] comparingArray)
        public static double CalcArraySimilarity(int[] originArray, int[] comparingArray)
        {
            if (originArray.Length > 0 && comparingArray.Length > 0)
            {
                int cnt = 0;

                foreach (var item in comparingArray)
                {
                    if (originArray.Contains(item))
                        cnt++;
                }

                return ((double)cnt / (double)Math.Max(originArray.Length, comparingArray.Length)) * 100.0;
            }
            else
            {
                return -1.0;
            }
        }

    }
}
