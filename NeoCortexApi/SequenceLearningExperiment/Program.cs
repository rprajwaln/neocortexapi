using NeoCortexApi;
using NeoCortexApi.DistributedComputeLib;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Network;


namespace SequenceLearningExperiment
{
    class Program
    {

        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger logger = loggerFactory.CreateLogger<Program>();
            MusicNotesExperiment(logger);
        }

        public static void MusicNotesExperiment(ILogger logger)
        {
            int inputBits = 100;
            int numColumns = 2048;
            List<double> inputValues = inputValues = new List<double>(new double[] { });
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();
            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = 25,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                InhibitionRadius = 15,

                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),

                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,

                // Learning is slower than forgetting in this case.
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,

                // Used by punishing of segments.
                PredictedSegmentDecrement = 0.1
            };
            
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
            
            // Stable and reached 100% accuracy with 2577 cycles
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 });

            // Stable and reached 100% accuracy with 2554 cycles
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 3.0, 2.0, 1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 0.0, 1.0, 2.0, 3.0, 3.0, 2.0, 1.0, 0.0, 1.0});

            // Stable and reached 100% accuracy with 3560 cycles
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0 });

            // Stable and reached 100% accuracy with 3401 cycles 
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0 });
            
            // Stable and 100% accuracy reached in with 2040 cycles 
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0 });

            // Stable and 100% accuracy reached in with 55 cycles 
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0, 12.0 });

            // Stable and reached 100% accuracy with 112 cycles. 
            // List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 15.0, 7.0, 5.0 });
            
            // Stable and reached 100% accuracy with 70 cycles.
            // var inputValues = new List<double>(new double[] {1.0,2.0,3.0,1.0,5.0,1.0,6.0});
            
            // Music Notes C-0, D-1, E-2, F-3, G-4, H-5
            // http://sea-01.cit.frankfurt-university.de:32224/?dmVyPTEuMDAxJiYwYzg1N2MyNmFmMzIyMjc0OD02MDlGQkRBOF83Njg0Nl8xNTU0N18xJiZmYjFlMzlhNWZmYWYyNDE9MTIzMyYmdXJsPWh0dHBzJTNBJTJGJTJGd3d3JTJFYmV0aHNub3Rlc3BsdXMlMkVjb20lMkYyMDEzJTJGMDglMkZ0d2lua2xlLXR3aW5rbGUtbGl0dGxlLXN0YXIlMkVodG1sJTBE
            // Stable and reached 100% accuracy with 1024 cycles.
            // var inputValues = new List<double>( new double[] { 0.0, 0.0, 4.0, 4.0, 5.0, 5.0, 4.0, 3.0, 3.0, 2.0, 2.0, 1.0, 1.0, 0.0 });

            // Stable and reached 100% accuracy with 531 cycles.
            // var inputValues = new List<double>(new double[] {0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0}

            // Sequence with multiple possibilties
            // Stable and reached 100% accuracy with 542 cycles.
            //var inputValues = new List<double>(new double[] {1.0, 2.0, 3.0, 4.0, 3.0, 2.0, 4.0, 5.0, 6.0, 1.0, 7.0});
            
            // Stable and reached 100% accuracy with 650 cycles.
            // var inputValues = new List<double>(new double[] { 2.0, 3.0, 2.0, 5.0, 2.0, 6.0, 2.0, 6.0, 2.0, 5.0, 2.0, 3.0, 2.0, 3.0, 2.0, 5.0, 2.0, 6.0 });

            // Calling Method to input values
            inputValues = InputSequence(inputValues, logger);
            RunExperiment(inputBits, cfg, encoder, inputValues, cls, logger);
        }



        /// <summary>
        ///
        /// </summary>
        private static void RunExperiment(int inputBits, HtmConfig cfg, EncoderBase encoder, List<double> inputValues, HtmClassifier<string, ComputeCycle> cls, ILogger logger)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int maxMatchCnt = 0;
            bool learn = true;

            CortexNetwork net = new CortexNetwork("my cortex");
            List<CortexRegion> regions = new List<CortexRegion>();
            CortexRegion region0 = new CortexRegion("1st Region");

            regions.Add(region0);

            var mem = new Connections(cfg);
            bool isInStableState;

            var numInputs = inputValues.Distinct<double>().ToList().Count;

            TemporalMemory tm1 = new TemporalMemory();

            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, numInputs * 55, (isStable, numPatterns, actColAvg, seenInputs) =>
            {
                if (isStable)
                    // Event should be fired when entering the stable state.
                    logger.LogInformation($"STABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");
                else
                    // Ideal SP should never enter unstable state after stable state.
                    logger.LogInformation($"INSTABLE: Patterns: {numPatterns}, Inputs: {seenInputs}, iteration: {seenInputs / numPatterns}");

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
            
            int[] prevActiveCols = new int[0];

            int cycle = 0;
            int matches = 0;

            string lastPredictedValue = "0";
            String prediction = null;

            Dictionary<double, List<List<int>>> activeColumnsLst = new Dictionary<double, List<List<int>>>();

            foreach (var input in inputs)
            {
                
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

                logger.LogInformation($"-------------- Cycle {cycle} ---------------");

                foreach (var input in inputs)
                {
                    logger.LogInformation($"-------------- {input} ---------------");

                    var lyrOut = layer1.Compute(input, learn) as ComputeCycle;

                    var activeColumns = layer1.GetResult("sp") as int[];

                    activeColumnsLst[input].Add(activeColumns.ToList());

                    previousInputs.Add(input.ToString());
                    if (previousInputs.Count > (maxPrevInputs + 1))
                        previousInputs.RemoveAt(0);

                    string key = GetKey(previousInputs, input);
                    
                    cls.Learn(key, lyrOut.ActiveCells.ToArray());

                    if (learn == false)
                        logger.LogInformation($"Inference mode");

                    logger.LogInformation($"Col  SDR: {Helpers.StringifyVector(lyrOut.ActivColumnIndicies)}");
                    logger.LogInformation($"Cell SDR: {Helpers.StringifyVector(lyrOut.ActiveCells.Select(c => c.Index).ToArray())}");

                    if (key == lastPredictedValue)
                    {
                        matches++;
                        logger.LogInformation($"Match. Actual value: {key} - Predicted value: {lastPredictedValue}");
                    }
                    else
                        logger.LogInformation($"Missmatch! Actual value: {key} - Predicted value: {lastPredictedValue}");

                    if (lyrOut.PredictiveCells.Count > 0)
                    {
                        var predictedInputValue = cls.GetPredictedInputValues(lyrOut.PredictiveCells.ToArray(), 3);
                        
                        logger.LogInformation($"Current Input: {input}");
                        logger.LogInformation("The predictions with similarity greater than 50% are");
                        
                        foreach (var t in predictedInputValue)
                        {
                            if (t.Similarity >= (double) 50.00)
                            {
                                logger.LogInformation($"Predicted Input: {string.Join(", ", t.PredictedInput)},\tSimilarity Percentage: {string.Join(", ", t.Similarity)}, \tNumber of Same Bits: {string.Join(", ", t.NumOfSameBits)}");
                            }
                        }
                        lastPredictedValue = predictedInputValue.First().PredictedInput;
                    }
                    else
                    {
                        logger.LogInformation($"NO CELLS PREDICTED for next cycle.");
                        lastPredictedValue = String.Empty;
                    }
                }
                

                double accuracy = (double)matches / (double)inputs.Length * 100.0;

                logger.LogInformation($"Cycle: {cycle}\tMatches={matches} of {inputs.Length}\t {accuracy}%");

                if (accuracy == 100.0)
                {
                    maxMatchCnt++;
                    logger.LogInformation($"100% accuracy reched {maxMatchCnt} times.");
                    if (maxMatchCnt >= 30)
                    {
                        sw.Stop();
                        logger.LogInformation($"Exit experiment in the stable state after 30 repeats with 100% of accuracy. Elapsed time: {sw.ElapsedMilliseconds / 1000 / 60} min.");
                        learn = false;
                        break;
                    }
                }
                else if (maxMatchCnt > 0)
                {
                    logger.LogInformation($"At 100% accuracy after {maxMatchCnt} repeats we get a drop of accuracy with {accuracy}. This indicates instable state. Learning will be continued.");
                    maxMatchCnt = 0;
                }
            }

            logger.LogInformation("---- cell state trace ----");

            cls.TraceState($"cellState_MinPctOverlDuty-{cfg.MinPctOverlapDutyCycles}_MaxBoost-{cfg.MaxBoost}.csv");

            logger.LogInformation("---- Spatial Pooler column state  ----");

            foreach (var input in activeColumnsLst)
            {
                using (StreamWriter colSw = new StreamWriter($"ColumState_MinPctOverlDuty-{cfg.MinPctOverlapDutyCycles}_MaxBoost-{cfg.MaxBoost}_input-{input.Key}.csv"))
                {
                    logger.LogInformation($"------------ {input.Key} ------------");

                    foreach (var actCols in input.Value)
                    {
                        logger.LogInformation(Helpers.StringifyVector(actCols.ToArray()));
                        colSw.WriteLine(Helpers.StringifyVector(actCols.ToArray()));
                    }
                }
            }

            logger.LogInformation("------------ END ------------");
            
            for (;;)
            {
                Inference(false, layer1, cls, logger);
            }

        }
        
        public static List<double> InputSequence( List<double> inputValues, ILogger logger)
        {
            logger.LogInformation("HTM Classifier is ready");
            logger.LogInformation("Please enter a sequence to be learnt");
            string userValue = Console.ReadLine();
            var numbers = userValue.Split(',');
            double sequence;
            foreach (var number in numbers)
            {
                if (double.TryParse(number, out sequence))
                {
                    inputValues.Add(sequence);
                }
            }
            return inputValues;
        }

        private static void Inference(bool learn, CortexLayer<object, object> layer1, HtmClassifier<string, ComputeCycle> cls, ILogger logger)
        {
            
                logger.LogInformation("\n Please enter a number that has been learnt or enter \"exit\" to exit");
                var input = (Console.ReadLine());
                if (input != "exit")
                {
                    int inputNumber = Convert.ToInt16(input);
                    var result = layer1.Compute(inputNumber, false) as ComputeCycle;
                    var predresult = cls.GetPredictedInputValues(result.PredictiveCells.ToArray(), 3);
                    logger.LogInformation("\n The predictions are:"); 
                    foreach (var ans in predresult)
                    { 
                        logger.LogInformation($"Predicted Input: {string.Join(", ", ans.PredictedInput)}," + 
                                          $"\tSimilarity Percentage: {string.Join(", ", ans.Similarity)}, " +
                                          $"\tNumber of Same Bits: {string.Join(", ", ans.NumOfSameBits)}");
                    }
                
                }
                else
                {
                    logger.LogInformation("You chose to exit. Goodbye!");
                    Environment.Exit(0); 
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
