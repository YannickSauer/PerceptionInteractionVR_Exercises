#if USE_VIVE

using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ViveSR.anipal.Eye;
public class ViveEyeTracker : IEyeTracker
{
    //private EyeCallbackHandler _callbackHandler;
    private static bool eye_callback_registered = false;
    private static Queue<GazeData> _gazeSamples;
    public void Initialize()
    {
        // Initialize Vive eye tracking SDK
        // TODO add framework initialization code here
    }

    public void Calibrate()
    {
        SRanipal_Eye.LaunchEyeCalibration();
    }

    public void StartBackgroundSampling(Queue<GazeData> gazeSamples)
    {
        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback && eye_callback_registered == false)
        {
            _gazeSamples = gazeSamples;
            eye_callback_registered = true;
            SRanipal_Eye.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
        }
    }

    public void StopBackgroundSampling()
    {
        if (eye_callback_registered == true)
        {
            SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }
    }

    internal class MonoPInvokeCallbackAttribute : System.Attribute
    {
        public MonoPInvokeCallbackAttribute() { }
    }

    /// <summary>
    /// Eye tracking data callback thread.
    /// Reports data at ~120hz
    /// MonoPInvokeCallback attribute required for IL2CPP scripting backend
    /// </summary>
    /// <param name="eye_data">Reference to latest eye_data</param>
    [MonoPInvokeCallback]
    public static void EyeCallback(ref EyeData eyeData)
    {
        GazeData gazeData = EyeData2GazeData(eyeData);
        _gazeSamples.Enqueue(gazeData);
        if (_gazeSamples.Count > 1000) // keep max queue length at 1000
        {
            _gazeSamples.Dequeue();
        }
    }

    public GazeData GetGazeData()
    {
        EyeData eyeData = new EyeData();
        SRanipal_Eye_API.GetEyeData(ref eyeData);
        return EyeData2GazeData(eyeData);
    }


    // transform SRAnipal eyeData to general GazeData struct
    // this also converts coordinate system direction and mm to m
    private static GazeData EyeData2GazeData(EyeData eyeData)
    {
        GazeData gazeData = new GazeData();

        // ET timestamp
        gazeData.deviceTimestamp = eyeData.timestamp;

        // validity
        gazeData.valid = eyeData.verbose_data.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) 
                      && eyeData.verbose_data.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY);
        
        // left eye
        gazeData.leftValidataBitMap = eyeData.verbose_data.left.eye_data_validata_bit_mask; // datatype ulong
        Vector3 origin = 0.001f * eyeData.verbose_data.left.gaze_origin_mm; // convert from mm to m
        origin.x = -origin.x; // mirror x-axis
        Vector3 direction = eyeData.verbose_data.left.gaze_direction_normalized;
        direction.x = -direction.x; // mirror x-axis
        gazeData.leftGazeRay = new Ray(origin, direction);
        gazeData.leftEyeOpenness = eyeData.verbose_data.left.eye_openness;
        gazeData.leftEyePupilDiameter = eyeData.verbose_data.left.pupil_diameter_mm;
        gazeData.leftPupilPosition = eyeData.verbose_data.left.pupil_position_in_sensor_area;

        // right eye
        gazeData.rightValidataBitMap = eyeData.verbose_data.right.eye_data_validata_bit_mask;
        origin = 0.001f * eyeData.verbose_data.right.gaze_origin_mm;
        origin.x = -origin.x;
        direction = eyeData.verbose_data.right.gaze_direction_normalized;
        direction.x = -direction.x;
        gazeData.rightGazeRay = new Ray(origin, direction);
        gazeData.rightEyeOpenness = eyeData.verbose_data.right.eye_openness;
        gazeData.rightEyePupilDiameter = eyeData.verbose_data.right.pupil_diameter_mm;
        gazeData.rightPupilPosition = eyeData.verbose_data.right.pupil_position_in_sensor_area;

        // combined gaze ray
        origin = 0.001f * eyeData.verbose_data.combined.eye_data.gaze_origin_mm;
        origin.x = -origin.x;
        direction = eyeData.verbose_data.combined.eye_data.gaze_direction_normalized;
        direction.x = -direction.x;
        gazeData.combinedGazeRay = new Ray(origin, direction);

        // gaze distance
        gazeData.gazeDistance = eyeData.verbose_data.combined.convergence_distance_mm * 0.001f;

        return gazeData;
    }
}
#endif