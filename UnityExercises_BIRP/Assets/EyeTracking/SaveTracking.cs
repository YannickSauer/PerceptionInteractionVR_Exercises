using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Text;

public class SaveTracking : MonoBehaviour
{
    public enum ETProvider
    {
        HTCViveSRanipal,
        Mouse
    }
    public ETProvider etprovider = ETProvider.Mouse;
    private IEyeTracker eyeTracker;

    // Enum to define different options for GameObject Tracking
    public enum TrackingOptions
    {
        localTransform,
        globalTransform,
    }

	public bool gazeRaycast = true; // check for raycast intersection with objects during runtime

    // Define a class to hold the dropdown option and associated GameObject
    [Serializable]
    public class TrackedObjectOptions
    {
        public TrackingOptions trackingOptions;
        public GameObject gameObject;
    }

    [Header("Object Tracking")]
    // List to hold the variables with dropdown options and associated GameObjects
    [SerializeField]
    private List<TrackedObjectOptions> trackedObjectList = new List<TrackedObjectOptions>();


    private string objectTrackingFile; // output file for object tracking (bound to framerate)
    private string gazeTrackingFile; // output file for eye tracking data (bound to eye tracking frequency)
    Queue trackingDataQueue = new Queue();
    Queue<GazeData> gazeTrackingQueue = new Queue<GazeData>();
    static string msgBuffer = "";
    private GazeData currentGazeData; // for gaze sample of the current frame
    private bool isObjectTracking = false;
    private bool isGazeTracking = false;
    private Thread savingThread; // background thread for writing to files

    void Start()
    {
        // set US culture for number formatting in strings
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

         switch (etprovider)
        {
            case ETProvider.HTCViveSRanipal:
                #if USE_VIVE
                eyeTracker = new ViveEyeTracker();
                eyeTracker.Initialize();
                #else
                Debug.LogError("Vive SRanipal not available. Please enable USE_VIVE in the project settings.");
                #endif
                // make sure the framework status is WORKING
                //Debug.Log(SRanipal_Eye_Framework.Status);
                //Debug.Log(SRanipal_Eye_Framework.FrameworkStatus.WORKING);
                break;
            case ETProvider.Mouse:
                eyeTracker = new MouseEyeTracker();
                eyeTracker.Initialize();
                Debug.Log("Mouse eye tracker initialized.");
                break;
            default:
                Debug.LogError("Unknown eye tracker provider selected.");
                break;
        }
    }
    void Update()
    {
        if(isGazeTracking)
        {
            currentGazeData = eyeTracker.GetGazeData();

        }
        if(isObjectTracking)
        {
            QueueTrackingData(trackingDataQueue);
        }
    }

    public GazeData GetGaze()
    {
        return eyeTracker.GetGazeData();
    }

    public void Initialize(string filename)
    {
        if (filename.Substring(filename.Length-4) != ".csv")
        {
            filename = filename + ".csv";
        }
        objectTrackingFile = filename.Substring(0,filename.Length-4) + ".csv";
        gazeTrackingFile = filename.Substring(0,filename.Length-4) + "_gaze.csv";

        // check if csv file already exists and change filename if necessary (e.g. _01...)
        int counter = 0;
        while (File.Exists(objectTrackingFile) || File.Exists(gazeTrackingFile))
        {
            counter++;
            objectTrackingFile = filename.Substring(0,filename.Length-4) + "_" + counter.ToString("D2") + ".csv";
            gazeTrackingFile = filename.Substring(0,filename.Length-4) + "_" + counter.ToString("D2") + "_gaze.csv";
        }
        WriteHeader();
        InvokeRepeating("Save", 0.0f, 1.0f); // save data to file every second
    }

    public void Calibrate()
    {
        Debug.Log("Starting eye tracking calibration");
        eyeTracker.Calibrate();
    }

    // start with queueing tracking data
    public void StartTracking()
    {
        isObjectTracking = true;
        if (!isGazeTracking)
        {
            isGazeTracking = true;
            Debug.Log("Start tracking.");
            eyeTracker.StartBackgroundSampling(gazeTrackingQueue);
        }
    }

    // stop tracking
    public void StopTracking()
    {
        isObjectTracking = false;
        isGazeTracking = false;
        eyeTracker.StopBackgroundSampling();
        WriteTrackingData(); // perform additioinal file writing to empty the queue
    }

    public void Msg(string msg)
    {
        msgBuffer = msg;
    }

    string GazeDataString(GazeData gazeDataSample)
    {
        StringBuilder datasetLine = new StringBuilder(350); // adjust capacity to your needs

        datasetLine.Append(gazeDataSample.deviceTimestamp.ToString() + ",");

        // left eye
        datasetLine.Append(gazeDataSample.leftValidataBitMap.ToString() + ",");
        datasetLine.Append(gazeDataSample.leftEyeOpenness.ToString("F10") + ",");
        datasetLine.Append(gazeDataSample.leftEyePupilDiameter.ToString("F10") + ",");
        datasetLine.Append(gazeDataSample.leftGazeRay.origin.x.ToString("F10") + "," + gazeDataSample.leftGazeRay.origin.y.ToString("F10") + "," + gazeDataSample.leftGazeRay.origin.z.ToString("F10") + ",");
        datasetLine.Append(gazeDataSample.leftGazeRay.direction.x.ToString("F10") + "," + gazeDataSample.leftGazeRay.direction.y.ToString("F10") + "," + gazeDataSample.leftGazeRay.direction.z.ToString("F10") + ",");
        datasetLine.Append(gazeDataSample.leftPupilPosition.x.ToString("F10") + "," + gazeDataSample.leftPupilPosition.y.ToString("F10") + ",");

        // right eye
        datasetLine.Append(gazeDataSample.rightValidataBitMap.ToString() + ",");
        datasetLine.Append(gazeDataSample.rightEyeOpenness.ToString("F10") + ",");
        datasetLine.Append(gazeDataSample.rightEyePupilDiameter.ToString("F10") + ",");
        datasetLine.Append(gazeDataSample.rightGazeRay.origin.x.ToString("F10") + "," + gazeDataSample.rightGazeRay.origin.y.ToString("F10") + "," + gazeDataSample.rightGazeRay.origin.z.ToString("F10") + ",");
        datasetLine.Append(gazeDataSample.rightGazeRay.direction.x.ToString("F10") + "," + gazeDataSample.rightGazeRay.direction.y.ToString("F10") + "," + gazeDataSample.rightGazeRay.direction.z.ToString("F10") + ",");
        datasetLine.Append(gazeDataSample.rightPupilPosition.x.ToString("F10") + "," + gazeDataSample.rightPupilPosition.y.ToString("F10") + ",");

        // combined eye
        datasetLine.Append(gazeDataSample.combinedGazeRay.origin.x.ToString("F10") + "," + gazeDataSample.combinedGazeRay.origin.y.ToString("F10") + "," + gazeDataSample.combinedGazeRay.origin.z.ToString("F10") + ",");
        datasetLine.Append(gazeDataSample.combinedGazeRay.direction.x.ToString("F10") + "," + gazeDataSample.combinedGazeRay.direction.y.ToString("F10") + "," + gazeDataSample.combinedGazeRay.direction.z.ToString("F10") + ",");
        datasetLine.Append(gazeDataSample.gazeDistance.ToString("F10") + ",");
        return(datasetLine.ToString());
    }



    void QueueTrackingData(Queue queue)
    {
        // StringBuilder should be quite effiction: https://stackoverflow.com/questions/21078/most-efficient-way-to-concatenate-strings
        StringBuilder datasetLine = new StringBuilder(700); // adjust capacity to your needs

        // timestamp: use time at beginning of frame
        datasetLine.Append(Time.time.ToString("F10") +",");

        // eye tracking timestampe
        datasetLine.Append(currentGazeData.deviceTimestamp.ToString() + ",");

		if (gazeRaycast)
		{
			datasetLine.Append(GazeRaycast());
		}

        // buffered message
        if (!String.IsNullOrEmpty(msgBuffer))
        {
            datasetLine.Append(msgBuffer + ",");
            msgBuffer = "";
        }
        queue.Enqueue(datasetLine.ToString());

    }

    void Save()
    {
        if (savingThread != null && savingThread.IsAlive)
        {
            Debug.Log("Previous saving thread is still running");
            return;
        }
        savingThread = new Thread(WriteTrackingData);
        savingThread.Start();
    }

    void WriteTrackingData()
    {
        int counter = 0;
        StreamWriter sw;
        string datasetLine;
        try
        {
            sw = new StreamWriter(objectTrackingFile, true); //true for append

            // dequeue trackingDataQueue until empty
            while (trackingDataQueue.Count > 0)
            {
                datasetLine = trackingDataQueue.Dequeue().ToString();
                counter++;
                sw.WriteLine(datasetLine); // write to file
            }
            sw.Close(); // close file
        }
        catch (Exception ex)
        {
            // Handle the exception (e.g., log it, display a message to the user, etc.)
            Console.WriteLine("An error occurred while writing to the file: " + ex.Message);
            // Optionally, you can log more detailed information about the exception:
            Console.WriteLine(ex.ToString());
        }


        if (isGazeTracking)
        {
            try
            {
                sw = new StreamWriter(gazeTrackingFile, true); //true for append
                // dequeue gazeTrackingQueue until empty
                counter = 0;
                while (gazeTrackingQueue.Count > 0)
                {
                    datasetLine = GazeDataString(gazeTrackingQueue.Dequeue());
                    sw.WriteLine(datasetLine); // write to file
                    counter++;
                }
                sw.Close(); // close file
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it, display a message to the user, etc.)
                Console.WriteLine("An error occurred while writing to the file: " + ex.Message);
                // Optionally, you can log more detailed information about the exception:
                Console.WriteLine(ex.ToString());
            }
        }

    }

	public string GazeRaycast()
    {
        RaycastHit hit;
	    Vector3 rayOrigin = Camera.main.transform.position + Camera.main.transform.rotation * currentGazeData.combinedGazeRay.origin;
	    Vector3 rayDirection = Camera.main.transform.rotation * currentGazeData.combinedGazeRay.direction;


        if (Physics.Raycast(rayOrigin, rayDirection, out hit))
        {
            return hit.point.x.ToString("F10") + "," + hit.point.y.ToString("F10") + "," + hit.point.z.ToString("F10") + ",";
        }
        else
        {
            return "NA,,,,";
        }
    }

    void WriteHeader()
    {
        StreamWriter sw = new StreamWriter(objectTrackingFile);

        // header for object tracking file
        string header = "timestamp,";
        header += "eye_timestamp,";

		if(gazeRaycast)
		{
			header += "hit_point.x,hit_point.y,hit_point.z,";
		}

        header += "messages,";
        sw.WriteLine(header);
        sw.Close();
    }
}
