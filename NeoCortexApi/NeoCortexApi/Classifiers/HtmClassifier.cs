﻿// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;

namespace NeoCortexApi.Classifiers
{
    /// <summary>
    /// Classifier implementation which memorize all seen values.
    /// </summary>
    /// <typeparam name="TIN"></typeparam>
    /// <typeparam name="TOUT"></typeparam>
    public class HtmClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
    {
        private int maxRecordedElements = 5;

        private List<TIN> inputSequence = new List<TIN>();

        private Dictionary<int[], int> inputSequenceMap = new Dictionary<int[], int>();

        private Dictionary<int[], TIN> activeMap = new Dictionary<int[], TIN>();

        /// <summary>
        /// Recording of all SDRs. See maxRecordedElements.
        /// </summary>
        private Dictionary<TIN, List<int[]>> m_AllInputs = new Dictionary<TIN, List<int[]>>();

        /// <summary>
        /// Mapping between the input key and the SDR assootiated to the input.
        /// </summary>
        private Dictionary<TIN, int[]> m_ActiveMap2 = new Dictionary<TIN, int[]>();


        public void ClearState()
        {
            m_ActiveMap2.Clear();
        }

        public void Learn(TIN input, Cell[] activeCells, bool learn)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Assotiate specified input to the given set of predictive cells.
        /// </summary>
        /// <param name="input">Any kind of input.</param>
        /// <param name="output">The SDR of the input as calculated by SP.</param>
        /// <param name="predictedOutput"></param>
        public void Learn(TIN input, Cell[] output)
        {
            // this.inputSequence.Add(input);

            var cellIndicies = GetCellIndicies(output);

            if (m_AllInputs.ContainsKey(input) == false)
                m_AllInputs.Add(input, new List<int[]>());

            // Record SDR
            m_AllInputs[input].Add(cellIndicies);

            // Make sure that only few last SDRs are recorded.
            if (m_AllInputs[input].Count > maxRecordedElements)
                m_AllInputs[input].RemoveAt(0);

            if (m_ActiveMap2.ContainsKey(input))
            {
                if (!m_ActiveMap2[input].SequenceEqual(cellIndicies))
                {
                    var numOfSameBitsPct = m_ActiveMap2[input].Intersect(cellIndicies).Count();
                    Console.WriteLine(
                        $"Prev/Now/Same={m_ActiveMap2[input].Length}/{cellIndicies.Length}/{numOfSameBitsPct}");
                }

                m_ActiveMap2[input] = cellIndicies;
            }
            else
                m_ActiveMap2.Add(input, cellIndicies);
        }

        public void Learn(TIN input, Cell[] output, Cell[] predictedOutput)
        {
            inputSequence.Add(input);

            inputSequenceMap.Add(GetCellIndicies(output), inputSequence.Count - 1);

            if (!activeMap.ContainsKey(GetCellIndicies(output)))
            {
                activeMap.Add(GetCellIndicies(output), input);
            }
        }

        public class ClassifierResult
        {
            public TIN PredictedInput { get; set; }

            /// <summary>
            /// Number of identical non-zero bits in the SDR.
            /// </summary>
            public int NumOfSameBits { get; set; }

            public double Similarity { get; set; }
        }


        public List<ClassifierResult> GetPredictedInputValues(Cell[] predictiveCells, short howMany)
        {
            
            List<ClassifierResult> res = new List<ClassifierResult>();
            double maxSameBits = 0;
            TIN predictedValue = default;
            Dictionary<TIN, ClassifierResult> dict = new Dictionary<TIN, ClassifierResult>();
            
            if (predictiveCells.Length != 0)
            {
                int indxOfMatchingInp = 0;
                Console.WriteLine($"Item length: {predictiveCells.Length}\t Items: {m_ActiveMap2.Keys.Count}");
                int n = 0;

                List<int> sortedMatches = new List<int>();

                var celIndicies = GetCellIndicies(predictiveCells);

                Console.WriteLine($"Predictive cells: {celIndicies.Length} \t {Helpers.StringifyVector(celIndicies)}");

                foreach (var pair in m_ActiveMap2)
                {
                    if (pair.Value.SequenceEqual(celIndicies))
                    {
                        res.Add(new ClassifierResult { PredictedInput = pair.Key, Similarity = (float)100.0, NumOfSameBits = pair.Value.Length});
                        
                    }
                    else
                    {
                        var numOfSameBitsPct = pair.Value.Intersect(celIndicies).Count();
                        double simPercentage = Math.Round(MathHelpers.CalcArraySimilarity(pair.Value, celIndicies), 2);
                        dict.Add(pair.Key, new ClassifierResult { PredictedInput = pair.Key, NumOfSameBits = numOfSameBitsPct, Similarity = simPercentage });
                        
                        if (numOfSameBitsPct > maxSameBits)
                        {
                            maxSameBits = numOfSameBitsPct;
                            predictedValue = pair.Key;
                            indxOfMatchingInp = n;
                        }
                    }
                    n++;
                }
            }

            int cnt = 0;
            foreach (var keyPair in dict.Values.OrderByDescending(key => key.Similarity))
            {
                res.Add(keyPair);
                    if (++cnt > howMany)
                        break;
            }

            return res;
        }
        

        /// <summary>
        /// Gets predicted value for next cycle
        /// </summary>
        /// <param name="predictiveCells">The list of predictive cells.</param>
        /// <returns></returns>
        [Obsolete("This method will be removed in the future. Use GetPredictedInputValues instead. ")]
        public TIN GetPredictedInputValue(Cell[] predictiveCells)
        {
            // bool x = false;
            double maxSameBits = 0;
            TIN predictedValue = default;
            if (predictiveCells.Length != 0)
            {
                int indxOfMatchingInp = 0;
                Console.WriteLine($"Item length: {predictiveCells.Length}\t Items: {m_ActiveMap2.Keys.Count}");
                int n = 0;

                List<int> sortedMatches = new List<int>();

                var celIndicies = GetCellIndicies(predictiveCells);

                Console.WriteLine($"Predictive cells: {celIndicies.Length} \t {Helpers.StringifyVector(celIndicies)}");

                foreach (var pair in m_ActiveMap2)
                {
                    if (pair.Value.SequenceEqual(celIndicies))
                    {
                        Console.WriteLine(
                            $">indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length}\tsimilarity 100pct\t {Helpers.StringifyVector(pair.Value)}");
                        return pair.Key;
                    }
                    
                    var numOfSameBitsPct = pair.Value.Intersect(celIndicies).Count();
                    if (numOfSameBitsPct > maxSameBits)
                    {
                        Console.WriteLine(
                            $">indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length} = similarity {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Value)}");
                        maxSameBits = numOfSameBitsPct;
                        predictedValue = pair.Key;
                        indxOfMatchingInp = n;
                    }
                    else
                        Console.WriteLine(
                            $"<indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length} = similarity {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Value)}");

                    n++;
                }
            }

            return predictedValue;
        }
        
        /// <summary>
        /// Traces out all cell indicies grouped by input value.
        /// </summary>
        public void TraceState(string fileName = null)
        {
            StreamWriter sw = null;
            if (fileName != null)
                sw = new StreamWriter(fileName);
            else
                sw = new StreamWriter(fileName.Replace(".csv", "HtmClassifier.state.csv"));

            List<TIN> processedValues = new List<TIN>();

            foreach (var item in m_ActiveMap2)
            {
                Console.WriteLine("");
                Console.WriteLine($"{item.Key}");
                Console.WriteLine($"{Helpers.StringifyVector(item.Value)}");

                sw.WriteLine("");
                sw.WriteLine($"{item.Key}");
                sw.WriteLine($"{Helpers.StringifyVector(item.Value)}");
            }

            if (sw != null)
            {
                sw.Flush();
                sw.Close();
            }

            Console.WriteLine("........... Cell State .............");

            using (var cellStateSw = new StreamWriter(fileName.Replace(".csv", "HtmClassifier.fullstate.csv")))
            {
                foreach (var item in m_AllInputs)
                {
                    Console.WriteLine("");
                    Console.WriteLine($"{item.Key}");

                    cellStateSw.WriteLine("");
                    cellStateSw.WriteLine($"{item.Key}");
                    foreach (var cellState in item.Value)
                    {
                        var str = Helpers.StringifyVector(cellState);
                        Console.WriteLine(str);
                        cellStateSw.WriteLine(str);
                    }
                }
            }
        }
        
        private string ComputeHash(byte[] rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(rawData);

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }


        private static byte[] FlatArray(Cell[] output)
        {
            byte[] arr = new byte[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                arr[i] = (byte)output[i].Index;
            }
            return arr;
        }

        private static int[] GetCellIndicies(Cell[] output)
        {
            int[] arr = new int[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                arr[i] = output[i].Index;
            }
            return arr;
        }

        private int PredictNextValue(int[] activeArr, int[] predictedArr)
        {
            var same = predictedArr.Intersect(activeArr);

            return same.Count();
        }


    }
}
