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


<img width="1320" alt="Screenshot 2021-06-27 at 20 55 38" src="https://user-images.githubusercontent.com/56980973/123556203-1f913200-d78a-11eb-867b-90840489f439.png">
<img width="1321" alt="Screenshot 2021-06-27 at 20 56 01" src="https://user-images.githubusercontent.com/56980973/123556209-26b84000-d78a-11eb-805e-7b2c45f74308.png">


The classifier is traversing through all memorized SDRs and tries to match the best ones. It is able detect complex sequences and tracks the list of inputs during the learning process.

1.	The classifier returns the array of possible inputs.
2.	The classifier also looks for the input and looks up the position of the classifier in the entire learning process.

Here the index 22,28,34 have a simliarity of 100. The HTM classifier provides a list of possible predicted inputs.

Method Signature

        /// <summary>
        /// Gets the list predicted inputs sorted by similarity.
        /// </summary>
        /// <param name="predictiveCells"></param>
        /// <param name="howMany">Specifies how many predicted SDRs should be reurned.</param>
        /// <returns></returns>
        public ICollection<ClassifierResult> GetPredictedInputValues(Cell[] predictiveCells, short howMany);

Here, howMany defines the number of top predictions that should be considered in the predicted list from the HTM Classifier.

For Example for the sequence 1, 2, 3, 4, 3, 2, 4, 5, 6

After the number 2, there are two possibilties of outcomes :

2-3  
2-4

The  HTM classfier method returns all possibilties as shown in following figure :

<img width="617" alt="Screenshot 2021-06-28 at 09 54 18" src="https://user-images.githubusercontent.com/56980973/123600309-d62afc00-d7f6-11eb-9cd4-09e1cd788647.png">

