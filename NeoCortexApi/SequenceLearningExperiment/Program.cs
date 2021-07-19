﻿using NeoCortexApi;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Network;

namespace SequenceLearningExperiment
{
    class Program
    {
        static void Main(string[] args)
        {
            MusicNotesExperiment();
        }

        public static void MusicNotesExperiment()
        {
            int inputBits = 100;
            int numColumns = 2048;
            Parameters p = Parameters.getAllDefaultParameters();

            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { numColumns });

            p.Set(KEY.CELLS_PER_COLUMN, 25);

            p.Set(KEY.GLOBAL_INHIBITION, true);
            p.Set(KEY.LOCAL_AREA_DENSITY, -1); // In a case of global inhibition.

            //p.setNumActiveColumnsPerInhArea(10);
            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            p.setNumActiveColumnsPerInhArea(0.02 * numColumns);
            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.Set(KEY.POTENTIAL_RADIUS, 50);
            p.setInhibitionRadius(15);

            //
            // Activates the high bumping/boosting of inactive columns.
            // This exeperiment uses HomeostaticPlasticityActivator, which will deactivate boosting and bumping.
            p.Set(KEY.MAX_BOOST, 10.0);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 25);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.75);

            // Max number of synapses on the segment.
            p.setMaxNewSynapsesPerSegmentCount((int)(0.02 * numColumns));

            // If learning process does not generate active segments, this value should be decreased. You can notice this with continious burtsing. look in trace for 'B.B.B'
            // If invalid patterns are predicted then this value should be increased.
            p.setActivationThreshold(15);
            p.setConnectedPermanence(0.5);

            // Learning is slower than forgetting in this case.
            p.setPermanenceDecrement(0.25);
            p.setPermanenceIncrement(0.15);

            // Used by punishing of segments.
            p.Set(KEY.PREDICTED_SEGMENT_DECREMENT, 0.1);

            double max = 20;

            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };

            EncoderBase encoder = new ScalarEncoder(settings);

            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, 0.0, 1.0, 2.0, 0.0, 1.0, 2.0, 2.0, 0.0, 0.1, 2.0 });
            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0 });
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.

            // not stable with 2048 cols 25 cells per column and 0.02 * numColumns synapses on segment.
            // Stable with permanence decrement 0.25/ increment 0.15 and ActivationThreshold 25.
            // With increment=0.2 and decrement 0.3 has taken 15 min and didn't entered the stable state.
           // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 });

            // Active Experiment
            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 3.0, 2.0, 1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 0.0, 1.0, 2.0, 3.0, 3.0, 2.0, 1.0, 0.0, 1.0});

            // Stable with 2048 cols 25 cells per column and 0.02 * numColumns synapses on segment.8min, 154 min, maxPrevInputs=5. connected permanence 0.35 or 0.5. PREDICTED_SEGMENT_DECREMENT= 0.1, permIncr = 0.15, permDecr=0.15, activationThreshold = 15
            // Stable with 2048 cols 25 cells per column and 0.02 * numColumns synapses on segment.8min, 9min.
            // not stable with 2048 cols 15 cells per column and 0.02 * numColumns synapses on segment.
            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0 });

            // Stable with 2048 cols AND 15 cells per column and 1000 0.02 * numColumns on segment. 7min,8min
            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0 });

            // not stable with 2048 cols 10 cells per column and 0.02 * numColumns synapses on segment.
            // Stable with 2048 cols AND 15 cells per column and 1000 0.02 * numColumns on segment. 9 min
            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0 });

            // Exit experiment in the stable state after 30 repeats with 100 % of accuracy.Elapsed time: 5 min and 55 cycles. 
            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 12.0 });

            // 112 cycles. Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: 8 min.
            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 15.0, 7.0, 5.0 });

            // 91.6% accuracy with 2048 with 15 cells per column.
            //                     3000 with 15 cells per column.
            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 15.0, 17.0, 11.00, 12.00, 17.00 });

            // C-0, D-1, E-2, F-3, G-4, H-5
            // http://sea-01.cit.frankfurt-university.de:32224/?dmVyPTEuMDAxJiYwYzg1N2MyNmFmMzIyMjc0OD02MDlGQkRBOF83Njg0Nl8xNTU0N18xJiZmYjFlMzlhNWZmYWYyNDE9MTIzMyYmdXJsPWh0dHBzJTNBJTJGJTJGd3d3JTJFYmV0aHNub3Rlc3BsdXMlMkVjb20lMkYyMDEzJTJGMDglMkZ0d2lua2xlLXR3aW5rbGUtbGl0dGxlLXN0YXIlMkVodG1sJTBE
            //var inputValues = new List<double>( new double[] { 0.0, 0.0, 4.0, 4.0, 5.0, 5.0, 4.0, 3.0, 3.0, 2.0, 2.0, 1.0, 1.0, 0.0 });

            
            //var inputValues = new List<double>(new double[] {  5.0, 3.0, 2.0, 5.0, 2.0, 6.0});
            
            //sequence with multiple possibilties
            
            var inputValues = new List<double>(new double[] {1.0, 2.0, 3.0, 4.0, 3.0, 2.0, 4.0, 5.0, 6.0, 1.0, 7.0});


            //var inputValues = new List<double>(new double[] { 2.0, 3.0, 2.0, 5.0, 2.0, 6.0, 2.0, 6.0, 2.0, 5.0, 2.0, 3.0, 2.0, 3.0, 2.0, 5.0, 2.0, 6.0 });

            RunExperiment(inputBits, p, encoder, inputValues);
        }



        /// <summary>
        ///
        /// </summary>
        private static void RunExperiment(int inputBits, Parameters p, EncoderBase encoder, List<double> inputValues)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int maxMatchCnt = 0;
            bool learn = true;

            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");

            regions.Add(region0);

            var mem = new Connections();

            p.apply(mem);

            bool isInStableState;

            //HtmClassifier<double, ComputeCycle> cls = new HtmClassifier<double, ComputeCycle>();
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            var numInputs = inputValues.Distinct<double>().ToList().Count;

            TemporalMemory tm1 = new TemporalMemory();

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, numInputs * 55, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    Debug.WriteLine($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    Debug.WriteLine($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

                if (numPatterns != numInputs)
                    throw new InvalidOperationException("Stable state must observe all input patterns");

                isInStableState = true;
                cls.ClearState();

                tm1.Reset(mem);
            }, numOfCyclesToWaitOnChange: 25);


            SpatialPoolerMT sp1 = new SpatialPoolerMT(hpa);
            sp1.Init(mem,  new DistributedMemory()
            {
                ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1),
            });

            tm1.Init(mem);

            CortexLayer<object, object> layer1 = new CortexLayer<object, object>("L1");
            region0.AddLayer(layer1);
            layer1.HtmModules.Add("encoder", encoder);
            layer1.HtmModules.Add("sp", sp1);
            layer1.HtmModules.Add("tm", tm1);

            double[] inputs = inputValues.ToArray();
            //Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();
            int[] prevActiveCols = new int[0];

            int cycle = 0;
            int matches = 0;

            string lastPredictedValue = "0";
            String prediction = null;

            Dictionary<double, List<List<int>>> activeColumnsLst = new Dictionary<double, List<List<int>>>();

            foreach (var input in inputs)
            {
                //prevActiveCols.Add(input, new int[0]);
                if (activeColumnsLst.ContainsKey(input) == false)
                    activeColumnsLst.Add(input, new List<List<int>>());
            }

            int maxCycles = 3500;
            int maxPrevInputs = inputValues.Count - 1;
            List<string> previousInputs = new List<string>();
            previousInputs.Add("-1.0");
            
            
            // Now training with SP+TM. SP is pretrained on the given input pattern.
            for (int i = 0; i < maxCycles; i++)
            {
                matches = 0;

                cycle++;

                Debug.WriteLine($"-------------- Cycle {cycle} ---------------");

                foreach (var input in inputs)
                {
                    Debug.WriteLine($"-------------- {input} ---------------");

                    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;

                    var activeColumns = layer1.GetResult("sp") as int[];

                    activeColumnsLst[input].Add(activeColumns.ToList());

                    previousInputs.Add(input.ToString());
                    if (previousInputs.Count > (maxPrevInputs + 1))
                        previousInputs.RemoveAt(0);

                    string key = GetKey(previousInputs, input);

                    //cls.Learn(GetKey(prevInput, input), lyrOut.ActiveCells.ToArray());
                    cls.Learn(key, lyrOut.ActiveCells.ToArray());

                    if (learn == false)
                        Debug.WriteLine($"Inference mode");

                    Debug.WriteLine($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                    Debug.WriteLine($"Cell SDR: {Helpers.StringifyVector(lyrOut.ActiveCells.Select(c => c.Index).ToArray())}");

                    if (key == lastPredictedValue)
                    {
                        matches++;
                        Debug.WriteLine($"Match. Actual value: {key} - Predicted value: {lastPredictedValue}");
                    }
                    else
                        Debug.WriteLine($"Missmatch! Actual value: {key} - Predicted value: {lastPredictedValue}");

                    if (lyrOut.PredictiveCells.Count > 0)
                    {
                        //var predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());
                        var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);
                        
                        Debug.WriteLine($"Current Input: {input}");
                        Debug.WriteLine("The predictions with similarity greater than 50% are");
                        
                        foreach (var t in predictedInputValue)
                        {
                            if (t.Similarity >= (double) 50.00)
                            {
                                Debug.WriteLine($"Predicted Input: {string.Join(", ", t.PredictedInput)},\tSimilarity Percentage: {string.Join(", ", t.Similarity)}, \tNumber of Same Bits: {string.Join(", ", t.NumOfSameBits)}");
                            }
                        }
                        lastPredictedValue = predictedInputValue.First().PredictedInput;
                    }
                    else
                    {
                        Debug.WriteLine($"NO CELLS PREDICTED for next cycle.");
                        lastPredictedValue = String.Empty;
                    }
                }

                // The brain does not do that this way, so we don't use it.
                // tm1.reset(mem);

                double accuracy = (double)matches / (double)inputs.Length * 100.0;

                Debug.WriteLine($"Cycle: {cycle}\tMatches={matches} of {inputs.Length}\t {accuracy}%");

                if (accuracy == 100.0)
                {
                    maxMatchCnt++;
                    Debug.WriteLine($"100% accuracy reched {maxMatchCnt} times.");
                    if (maxMatchCnt >= 30)
                    {
                        sw.Stop();
                        Debug.WriteLine($"Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                        learn = false;
                        //var testInputs = new double[] { 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 0.0, 1.0 };

                        // C-0, D-1, E-2, F-3, G-4, H-5
                        //var testInputs = new double[] { 0.0, 0.0, 4.0, 4.0, 5.0, 5.0, 4.0, 3.0, 3.0, 2.0, 2.0, 1.0, 1.0, 0.0 };

                        //// Traverse the sequence and check prediction.
                        //foreach (var input in inputValues)
                        //{
                        //    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;
                        //    predictedInputValue = cls.GetPredictedInputValue(lyrOut.predictiveCells.ToArray());
                        //    Debug.WriteLine($"I={input} - P={predictedInputValue}");
                        //}
                        /*
                        //
                        // Here we let the HTM predict sequence five times on its own.
                        // We start with last predicted value.
                        int cnt = 5 * inputValues.Count;

                        Debug.WriteLine("---- Start Predicting the Sequence -----");

                        //
                        // This code snippet starts with some input value and tries to predict all next inputs
                        // as they have been learned as a sequence.
                        // We take a random value to start somwhere in the sequence.
                        var predictedInputValue = inputValues[new Random().Next(0, inputValues.Count - 1)].ToString();

                        List<string> predictedValues = new List<string>();

                        while (--cnt > 0)
                        {
                            //var lyrOut = layer1.Compute(predictedInputValue, learn) as ComputeCycle;
                            var lyrOut = layer1.Compute(double.Parse(predictedInputValue[predictedInputValue.Length - 1].ToString()), false) as ComputeCycle;
                            predictedInputValue = cls.GetPredictedInputValue(lyrOut.PredictiveCells.ToArray());
                            predictedValues.Add(predictedInputValue);
                        };

                        // Now we have a sequence of elements and watch in the trace if it matches to defined input set.
                        foreach (var item in predictedValues)
                        {
                            Debug.Write(item);
                            Debug.Write(" ,");
                        }*/
                        break;
                    }
                }
                else if (maxMatchCnt > 0)
                {
                    Debug.WriteLine($"At 100% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with {accuracy}. This indicates instable state. Learning will be continued.");
                    maxMatchCnt = 0;
                }
            }

            Debug.WriteLine("---- cell state trace ----");

            cls.TraceState($"cellState_MinPctOverlDuty-{p[KEY.MIN_PCT_OVERLAP_DUTY_CYCLES]}_MaxBoost-{p[KEY.MAX_BOOST]}.csv");

            Debug.WriteLine("---- Spatial Pooler column state  ----");

            foreach (var input in activeColumnsLst)
            {
                using (StreamWriter colSw = new StreamWriter($"ColumState_MinPctOverlDuty-{p[KEY.MIN_PCT_OVERLAP_DUTY_CYCLES]}_MaxBoost-{p[KEY.MAX_BOOST]}_input-{input.Key}.csv"))
                {
                    Debug.WriteLine($"------------ {input.Key} ------------");

                    foreach (var actCols in input.Value)
                    {
                        Debug.WriteLine(Helpers.StringifyVector(actCols.ToArray()));
                        colSw.WriteLine(Helpers.StringifyVector(actCols.ToArray()));
                    }
                }
            }

            Debug.WriteLine("------------ END ------------");
            learntSequence(2,false, layer1, cls);
            
        }

        private static void learntSequence(int input, bool learn, CortexLayer<object, object> layer1, HtmClassifier<string, ComputeCycle> cls)
        {
            var result = layer1.Compute(input, learn) as ComputeCycle;

            var predresult = cls.GetPredictedInputValues(result.PredictiveCells.ToArray(), 2);
            foreach (var ans in predresult)
            {
                if (ans.Similarity >= (double) 50.00)
                {
                    Debug.WriteLine($"Predicted Input: {string.Join(", ", ans.PredictedInput)},\tSimilarity Percentage: {string.Join(", ", ans.Similarity)}, \tNumber of Same Bits: {string.Join(", ", ans.NumOfSameBits)}");
                }
            }
        }

        private static string GetKey(List<string> prevInputs, double input)
        {
            string key = String.Empty;

            for (int i = 0; i < prevInputs.Count; i++)
            {
                if (i > 0)
                    key += "-";

                key += (prevInputs[i]);
            }

            return key;
        }
    }
}
