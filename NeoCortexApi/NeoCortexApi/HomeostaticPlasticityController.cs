﻿// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NeoCortexApi
{
    /// <summary>
    /// Implements the new-born effect in the SP. This effect tracks the learning process of the SP 
    /// and switches-off the boosting mechanism (new-born effect) after the SP has entered a stable state 
    /// for all seen input patterns.
    /// </summary>
    public class HomeostaticPlasticityController
    {
        private int m_MaxPreviousElements = 5;

        private Connections m_HtmMemory;

        private int m_Cycle = 0;

        private int m_MinCycles;

        /// <summary>
        /// Number of minimal required stable cycles for every input.
        /// When this value is reached, the new-born effect will take a place.
        /// It means, the boosting will be disabled.
        /// </summary>
        private int m_RequiredNumOfStableCycles;

        /// <summary>
        /// Number stable cycles for every input.
        /// </summary>
        private Dictionary<string, int> m_NumOfStableCyclesForInput = new Dictionary<string, int>();

        /// <summary>
        /// Number of active columns in SDRs of last N steps.
        /// </summary>
        private Dictionary<string, int[]> m_NumOfActiveColsForInput = new Dictionary<string, int[]>();

        /// <summary>
        /// Keeps the list of hash values of all seen input patterns.
        /// List of hashes. [key, val] = [hash(input), hash(output)]
        /// </summary>
        private Dictionary<string, int[]> m_InOutMap = new Dictionary<string, int[]>();

        /// <summary>
        /// Action to be invoked when the SP become stable or instable.
        /// <is stable, num of seen patterns, derivation of active column function, cycle>
        /// </summary>
        private Action<bool, int, double, int> m_OnStabilityStatusChanged;

        /// <summary>
        /// Set on true when SP deactivates boosting and enter the stable state.
        /// Once SP enters the stable state and it becomes instable again, this value is set on false.
        /// </summary>
        private bool m_IsStable = false;

        /// <summary>
        /// Creates the instance of the HPC to stabilize the Spatial Pooler learning process.
        /// </summary>
        /// <param name="htmMemory">The initialized HTM memory.</param>
        /// <param name="minCycles">The minimume required cycles to keep boosting. This parameter defiens the new-born stage of the SP.
        /// During this period, the SP will boost columns and bee instable. After this period the HPC will switch off boosting.</param>
        /// <param name="onStabilityStatusChanged">Invoked when the SP changes the state from instable to stable and vise versa.</param>
        /// <param name="numOfCyclesToWaitOnChange">How many cycles SDRs of all input patterns must be unchanged to declare the SP as stable.</param>
        /// <summary>
        /// Used during the deserialization proicess.
        /// </summary>
        public HomeostaticPlasticityController()
        { 
        
        }

        /// <summary>
        /// Creates the instance of HomeostaticPlasticityController.
        /// </summary>
        /// <param name="htmMemory">The HTM memory.</param>
        /// <param name="minCycles">The minimum calls to the Learn method until HPC algorithm is activated. When this number is reached the HPC will disable boosting in SP. </param>
        /// <param name="onStabilityStatusChanged">Action invoked when the SP status is changed from stable t unstable and vise versa.</param>
        /// <param name="numOfCyclesToWaitOnChange">How many cycles all seen patterns must not change to declare SP as stable. Using smaller numbers might cause frequent status change.
        /// Higher numbers ensure more stable SP, but it takes longer time to enter the stable stabe.</param>
        public HomeostaticPlasticityController(Connections htmMemory, int minCycles, Action<bool, int, double, int> onStabilityStatusChanged, int numOfCyclesToWaitOnChange = 50)
        {
            this.m_OnStabilityStatusChanged = onStabilityStatusChanged;
            this.m_HtmMemory = htmMemory;
            this.m_MinCycles = minCycles;
            this.m_RequiredNumOfStableCycles = numOfCyclesToWaitOnChange;
        }

        /// <summary>
        /// Invoked as the last step in learning of the SP.
        /// </summary>
        /// <param name="input">The input of the SP in the current cycle.</param>
        /// <param name="output">The output SDR of the Spatial Pooler compute cycle.</param>
        /// <returns></returns>
        public bool Compute(int[] input, int[] output)
        {
            bool res = false;

            double avgDerivation = -1;

            // We take the hash value of the input.
            var inpHash = GetHash(input);

            //
            // Here we track the number of active columns for every cycle for every input.
            // We want that this number for every input is approximately the same.
            if (m_NumOfActiveColsForInput.ContainsKey(inpHash))
                ArrayUtils.PushToInterval(m_NumOfActiveColsForInput[inpHash], m_MaxPreviousElements, output.Count(c => c == 1));

            //
            // If the pattern appears for the first time, add it to dictionary of seen patterns.
            if (!m_InOutMap.ContainsKey(inpHash))
            {
                m_InOutMap.Add(inpHash, output);
                m_NumOfActiveColsForInput.Add(inpHash, new int[m_MaxPreviousElements]);
                m_NumOfStableCyclesForInput.Add(inpHash, 0);
            }
            else
            {
                if (m_Cycle >= this.m_MinCycles)
                {
                    //this.htmMemory.setMaxBoost(0.0);
                    this.m_HtmMemory.HtmConfig.MaxBoost = 0.0;

                    //this.htmMemory.updateMinPctOverlapDutyCycles(0.0);
                    this.m_HtmMemory.HtmConfig.MinPctOverlapDutyCycles = 0.0;

                    this.m_HtmMemory.HtmConfig.MinPctActiveDutyCycles = 0.0;
                }

                // If the input has been already seen, we calculate the similarity between already seen input
                // and the new input. The similarity is calculated as a correlation function.
                var similarity = Correlate(ArrayUtils.IndexWhere(m_InOutMap[inpHash], k => k == 1), ArrayUtils.IndexWhere(output, k => k == 1));

                // We replace the existing value with the new one.
                m_InOutMap[inpHash] = output;

                //
                // We cannot expect the 100% for the entire learning cycle. Sometimes some
                // SDR appear with few more or less bits than in the previous cycle.
                // If this happen we take the new SDR (output) as the winner and put it in the map.
                if (similarity > 0.96)
                {
                    // We calculate here the average change of the SDR for the given input.
                    avgDerivation = ArrayUtils.AvgDelta(m_NumOfActiveColsForInput[inpHash]);

                    //
                    // If there is no change (SDR is stable) we count nuber of stable cycles.
                    // If the average value is not 0, then we reset the number of stable cycles.
                    if (avgDerivation == 0)
                        m_NumOfStableCyclesForInput[inpHash] = m_NumOfStableCyclesForInput[inpHash] + 1;
                    else
                        m_NumOfStableCyclesForInput[inpHash] = 0;

                    if (m_Cycle >= this.m_MinCycles)
                    {
                        if (m_NumOfStableCyclesForInput[inpHash] > m_RequiredNumOfStableCycles && IsInStableState(m_NumOfStableCyclesForInput, m_RequiredNumOfStableCycles))
                        {
                            // We fire event when changed from instable to stable.
                            if (!m_IsStable)
                                this.m_OnStabilityStatusChanged(true, m_InOutMap.Keys.Count, avgDerivation, m_Cycle);

                            m_IsStable = true;
                            res = true;
                        }
                    }
                }
                else
                {
                    m_NumOfStableCyclesForInput[inpHash] = 0;

                    // If the new SDR output for the already seen input

                    if (m_IsStable)
                    {
                        // THIS SHOULD NEVER HAPPEN! MEANS FROM STABLE TO INSTABLE!
                        m_IsStable = false;
                        this.m_OnStabilityStatusChanged(false, m_InOutMap.Keys.Count, avgDerivation, m_Cycle);
                    }
                }
            }

            this.m_Cycle++;

            return res;
        }

        private static bool IsInStableState(Dictionary<string, int> stableCyclesForAllInputs, int requiredNumOfStableCycles)
        {
            bool res = true;

            foreach (var stableCycles in stableCyclesForAllInputs.Values)
            {
                if (stableCycles < requiredNumOfStableCycles)
                {
                    res = false;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Calculates the correlation of bits in ywo arrays.
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns>Similarity of two arrays.</returns>
        public static double Correlate(int[] data1, int[] data2)
        {
            double min = Math.Min(data1.Length, data2.Length);
            double max = Math.Max(data1.Length, data2.Length);

            double sum = 0;

            for (int i = 0; i < min; i++)
            {
                if (data1[i] == data2[i])
                    sum++;
            }

            return sum / max;
        }

        /// <summary>
        /// Compute the hash from the array.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static string GetHash(int[] input)
        {
            List<byte> buff = new List<byte>();

            foreach (var item in input)
            {
                buff.AddRange(BitConverter.GetBytes(item));
            }

            using (SHA256 hashAlgorithm = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = hashAlgorithm.ComputeHash(buff.ToArray());

                return Encoding.UTF8.GetString(data);
            }
        }

        /// <summary>
        /// Traces out all cell indicies grouped by input value.
        /// </summary>
        public void TraceState(string fileName = null)
        {
            if (fileName == null)
                fileName = $"{nameof(HomeostaticPlasticityController)}.state.csv";

            Debug.WriteLine("........... Column State .............");

            int cnt = 0;

            using (var cellStateSw = new StreamWriter(fileName))
            {
                foreach (var item in m_InOutMap)
                {
                    //string keyStr = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Key));
                    //string res = $"{cnt++}- stable cycles: {this.m_NumOfStableCyclesForInput.Count}";

                    var sdr = Helpers.StringifyVector(ArrayUtils.IndexWhere(m_InOutMap[item.Key], k => k == 1));

                    string str = $"[{cnt++} - stable cycles: {this.m_NumOfStableCyclesForInput[item.Key]},len = {m_InOutMap[item.Key].Count(l=>l==1)}] \t {sdr}";

                    //Debug.WriteLine(keyStr);
                    //Debug.WriteLine($"{res}");
                    Debug.WriteLine(str);

                    //cellStateSw.WriteLine($"{res} \t {keyStr}");
                    cellStateSw.WriteLine(str);
                }
            }
        }


        #region Serialization
        public void Serialize(StreamWriter writer)
        {
            HtmSerializer2 ser = new HtmSerializer2();

            ser.SerializeBegin(nameof(HomeostaticPlasticityController), writer);

            ser.SerializeValue(this.m_MaxPreviousElements, writer);
            // m_HtmMemory is not serialized here. It is assumed to be serialized in the SP;
            ser.SerializeValue(this.m_Cycle, writer);
            ser.SerializeValue(this.m_MinCycles, writer);
            ser.SerializeValue(this.m_RequiredNumOfStableCycles, writer);
            ser.SerializeValue(this.m_NumOfStableCyclesForInput, writer);
            ser.SerializeValue(this.m_NumOfActiveColsForInput, writer);
            ser.SerializeValue(this.m_InOutMap, writer);
            ser.SerializeValue(this.m_IsStable, writer);

            ser.SerializeEnd(nameof(HomeostaticPlasticityController), writer);

        }

        public static HomeostaticPlasticityController Deserialize(StreamReader reader)
        {
            HomeostaticPlasticityController ctrl = new HomeostaticPlasticityController();

            HtmSerializer2 ser = new HtmSerializer2();
            //ctrl.m_MaxPreviousElements = ser.ReadIntValue(reader);
            //ctrl.m_MinCycles = ser.ReadIntValue(reader);
            //ctrl.m_RequiredNumOfStableCycles = ser.ReadIntValue(reader);
            ////...

            return ctrl;

        }
        #endregion
    }
}
