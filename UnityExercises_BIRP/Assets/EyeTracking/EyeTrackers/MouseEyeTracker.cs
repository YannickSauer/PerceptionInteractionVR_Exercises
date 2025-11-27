using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEyeTracker : IEyeTracker
{
    private GazeData currentGazeData;
    private bool isBackgroundTracking = false;
    private static Queue<GazeData> _gazeSamples;
    private static MonoBehaviour _mb = GameObject.FindObjectOfType<MonoBehaviour>();

    public void Initialize()
    {
        Debug.Log("Mouse eye tracking initialized.");
    }

    public void Calibrate()
    {
        Debug.Log("Mouse eye tracking does not require calibration.");
    }

    public GazeData GetGazeData()
    {
        if (isBackgroundTracking)
        {
            return currentGazeData;
        }
        else
        {
            return GenerateMouseGazeData();
        }
    }

    private IEnumerator QueueGazeData()
    {
        while (isBackgroundTracking)
        {
            GazeData gazeData = GenerateMouseGazeData();
            _gazeSamples.Enqueue(gazeData);
            if (_gazeSamples.Count > 1000)
            {
                _gazeSamples.Dequeue();
            }

            currentGazeData = gazeData;
            yield return null;

            if (!isBackgroundTracking)
                break;
        }
        Debug.Log("Stopped background gaze sampling.");
    }

    public void StartBackgroundSampling(Queue<GazeData> gazeSamples)
    {
        isBackgroundTracking = true;
        _gazeSamples = gazeSamples;
        Debug.Log("Starting background gaze sampling.");
        _mb.StartCoroutine(QueueGazeData());
    }

    public void StopBackgroundSampling()
    {
        isBackgroundTracking = false;
    }

    private GazeData GenerateMouseGazeData()
    {
        GazeData gazeData = new GazeData();
        Vector3 mousePosition = Input.mousePosition;
        Ray gazeRay = Camera.main.ScreenPointToRay(mousePosition);

        gazeData.deviceTimestamp = System.DateTime.Now.Ticks;
        gazeData.valid = true;
        gazeData.leftValidataBitMap = 2;
        gazeData.rightValidataBitMap = 2;
        gazeData.leftGazeRay = gazeRay;
        gazeData.rightGazeRay = gazeRay;
        gazeData.combinedGazeRay = gazeRay;
        gazeData.gazeDistance = 1.0f; // Default distance for debugging

        return gazeData;
    }
}