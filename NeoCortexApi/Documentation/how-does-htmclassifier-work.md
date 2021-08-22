## HtmClassifier
The HtmClassifier is a helper module that is used to help the process of the next element in the process of learning sequences.
The classifier provides two methods:

Learn(string key, int[] sdr)

GetPredictedInputValues(Cell[] predictiveCells, short howMany)

The method learn receives the key string, that represents the sequence and memorizes the SDR for the given key.
Assume, we learn following sequence: 
~~~
1-2-3-4-5-3-5
~~~

In every cycle, the experiment creates the key that represents the sequence in that cycle. For example, the key might look like:

Cycle 1: '1-2-3-4-5-3-5' , 
Cycle 2: '2-3-4-5-3-5-1', 
Cycle 3: '3-4-5-3-5-1-2', 
etc..

During the learning process, the input in every cycle is SDR of cells produced by Temporal Memory algorithm. Because the same SP output (column SDR) for some element (i.e.: ‘3’) will be represented in TM by a different set of cells inside of the same column set. SP generates always (if stable) the same set of active columns for the same element. However, TM does not generate the same set of active cells for the same element. The TM is trying to build the context of the element.
That means ‘3’ followed by ‘2’ produces a different set of active cells than ‘3’ followed by ‘5’. This is why the classifier gets the key in the form shown above. However, developers are free to build a key some other way.

The following shows the trace output of the learning process.

The classifier is traversing through all memorized SDRs and tries to match the best ones. It is able detect complex sequences and tracks the list of inputs during the learning process.

1.	The classifier returns the array of possible inputs.
2.	The classifier also looks for the input and looks up the position of the classifier in the entire learning process.

Method Signature

        public List<ClassifierResult> GetPredictedInputValues(Cell[] predictiveCells, short howMany)
        {
            List<ClassifierResult> res = new List<ClassifierResult>();
            double maxSameBits = 0;
            TIN predictedValue = default;
            Dictionary<TIN, ClassifierResult> dict = new Dictionary<TIN, ClassifierResult>();
         }

The implemented method ‘GetPredcitedInputValues’ in HTM classifier provides a list of possible predicted inputs. Here ‘howMany’ parameter defines the number of top predictions that should be considered in the predicted list from the HTM Classifier.


The following figure shows the trace for  sequence and here the index 0,6,12 have a similarity of 100. The classifier implementation provides top three possible outcomes. 
1. 2-3
2. 2-4
3. 2-6

![image](https://user-images.githubusercontent.com/56980973/130371328-76e191e0-3009-46b1-90d2-4bb5812215c6.png)


Once the classifier has learnt the sequence, you can just by entering the number in the sequence, then it would call the ‘Inference’ method and would list out the possible predicted outputs.

Method Signature

      private static List<double> InputSequence( List<double> inputValues)
       {
            Console.WriteLine("HTM Classifier is ready");
            Console.WriteLine("Please enter a sequence to be learnt");
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
        
Once the classifier has learnt the sequence, you can just by entering the number in the sequence, then it would call the ‘Inference’ method and would list out the possible predicted outputs.

Method Signature

       private static void Inference(int input, bool learn, CortexLayer<object, object> layer1, HtmClassifier<string, ComputeCycle> cls)
        {
            var result = layer1.Compute(input, false) as ComputeCycle;
            var predresult = cls.GetPredictedInputValues(result.PredictiveCells.ToArray(), 3);
            Console.WriteLine("\n The predictions are:");
            foreach (var ans in predresult)
            {
                Console.WriteLine($"Predicted Input: {string.Join(", ", ans.PredictedInput)}," +
                                  $"\tSimilarity Percentage: {string.Join(", ", ans.Similarity)}, " +
                                  $"\tNumber of Same Bits: {string.Join(", ", ans.NumOfSameBits)}");
            }
        }


Now the implemented HTM classifier method returns all possibilities as shown in following figure:
![image](https://user-images.githubusercontent.com/56980973/130371205-6a50e104-6378-404d-a36a-84aa030c175a.png)
  
![image](https://user-images.githubusercontent.com/56980973/130371208-6c412bb8-4324-4204-b3a2-32014c4177a0.png)

The results show that the proposed classifiers enhance the classification performance of HTM-CLA and their performance.




