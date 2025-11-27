using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gaze data struct
public struct GazeData
{
    public long deviceTimestamp;
    public bool valid;
    // Left Eye
    public ulong leftValidataBitMap;
    public Ray leftGazeRay;
    public float leftEyeOpenness;
    public float leftEyePupilDiameter;
    public Vector2 leftPupilPosition;

    // Right Eye
    public ulong rightValidataBitMap;
    public Ray rightGazeRay;
    public float rightEyeOpenness;
    public float rightEyePupilDiameter;
    public Vector2 rightPupilPosition;

    // combined
    public Ray combinedGazeRay;
    public float gazeDistance;
}
public interface IEyeTracker
{
    // Initialize the eye tracker
    void Initialize();

    // Calibrate eye tracking
    void Calibrate();

    // Get the current gaze point
    GazeData GetGazeData();

    // Start queueing Eye samples in background
    void StartBackgroundSampling(Queue<GazeData> gazeSamples);
    void StopBackgroundSampling();
}