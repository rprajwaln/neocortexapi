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

![image](https://user-images.githubusercontent.com/56980973/130371113-21f4ef8b-6e39-49c2-af95-f4da7ef24e53.png)

The implemented method ‘GetPredcitedInputValues’ in HTM classifier provides a list of possible predicted inputs. Here ‘howMany’ parameter defines the number of top predictions that should be considered in the predicted list from the HTM Classifier.


The following figure shows the trace for  sequence and here the index 0,6,12 have a similarity of 100. The classifier implementation provides top three possible outcomes. 
•	2-3
•	2-4
•	2-6

![image](https://user-images.githubusercontent.com/56980973/130371180-74d9510b-2fb4-4719-b075-5ebefd327f86.png)

Once the classifier has learnt the sequence, you can just by entering the number in the sequence, then it would call the ‘Inference’ method and would list out the possible predicted outputs.

![image](https://user-images.githubusercontent.com/56980973/130371191-74005884-d1c1-4b89-9d20-7402c23a6a52.png)

Now the implemented HTM classifier method returns all possibilities as shown in following figure:
![image](https://user-images.githubusercontent.com/56980973/130371205-6a50e104-6378-404d-a36a-84aa030c175a.png)
![image](https://user-images.githubusercontent.com/56980973/130371208-6c412bb8-4324-4204-b3a2-32014c4177a0.png)

The results show that the proposed classifiers enhance the classification performance of HTM-CLA and their performance.




