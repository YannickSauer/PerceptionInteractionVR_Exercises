using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(Camera))]
public class FoveatedRendering : MonoBehaviour
{
    public bool active = true;

    public float foveaRadius = 0.1f;
    public float maxBlurRadius = 4.0f;
    public float transitionSize = 0.1f;

    private Material foveatedRenderingMaterial;
    private SaveTracking eyeTracking;
    private struct FixationData
    {
        public float timestamp;
        public Vector2 gazeLoc;

        public FixationData(float time, Vector2 loc)
        {
            timestamp = time;
            gazeLoc = loc;
        }
    }
    private Queue<FixationData> gazeBuffer = new Queue<FixationData>(); // used for storing gaze samples that can be choosen for artifical delay stimulation
    private Vector2 prevData = new Vector2(0.5f,0.5f);
    
    public Vector2 gazeUV = new Vector2(0.5f, 0.5f);

    public float eyeTrackingDelay = 0f;

    
    public void SetGaze(Vector2 uv)
    {
        gazeUV = uv;
    }

    public void SetDelay(float delay)
    {
        eyeTrackingDelay = delay;
    }


    void Start()
    {
        foveatedRenderingMaterial = new Material(Shader.Find("Hidden/FoveatedRendering"));
        eyeTracking = GetComponent<SaveTracking>();
        eyeTracking.Initialize("FovRend");
    }

    void Update()
    {
        FixationData delayedSample;
        if (eyeTrackingDelay == 0f)
        {
             SetGaze(GetGazeSample().gazeLoc);
        }
        else
        {
            QueueGazeDate();
            if (TryGetDelayedSample(Time.time, eyeTrackingDelay, out delayedSample))
            {
                SetGaze(delayedSample.gazeLoc);
            }
        }
        
    }

    void QueueGazeDate()
    {
        gazeBuffer.Enqueue(GetGazeSample());
    }

    FixationData GetGazeSample()
    {
        GazeData currentGazeData = eyeTracking.GetGaze();
        Vector3 vp;
        // check if valid sample
        if (currentGazeData.valid)
        {
            Vector3 point = Camera.main.transform.TransformPoint(currentGazeData.combinedGazeRay.origin) +
                            Camera.main.transform.TransformDirection(currentGazeData.combinedGazeRay.direction) * 1.0f; 
            vp = Camera.main.WorldToViewportPoint(point);
            prevData = vp;
        }
        else
        {
            vp = prevData;
        }
        FixationData sample = new FixationData(
            Time.time,      // timestamp in seconds
            vp              // whatever source you use
        );
        return sample;
    }

    private bool TryGetDelayedSample(float currentTime, float delay, out FixationData delayedSample)
    {
        delayedSample = default;
        bool foundSample = false;
        // remove samples that are too old
        while (gazeBuffer.Count > 0)
        {
            FixationData oldest = gazeBuffer.Peek(); // check the oldest sample in the queue (wihtout removing)

            float age = currentTime - oldest.timestamp;

            if (age >= delay)
            {
                // this is the sample that matches the delay criterion
                delayedSample = gazeBuffer.Dequeue();
                foundSample = true;
                // still continue in case we find "younger" samples that are still >= delay
            }
            else
            {
                if (foundSample)
                    return true;
                else
                    return false;
                    // The next earliest sample is not old enough.
                // Stop here. We wait for more time to pass.
            }
        }

        return false; // seems like there is no sample in the queue

    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (!active || foveatedRenderingMaterial == null)
        {
            Graphics.Blit(src, dest);
            return;
        }

        foveatedRenderingMaterial.SetVector("_GazePos", gazeUV);
        foveatedRenderingMaterial.SetFloat("_FoveaRadius", foveaRadius);
        foveatedRenderingMaterial.SetFloat("_MaxBlurRadius", maxBlurRadius);
        foveatedRenderingMaterial.SetFloat("_TransitionSize", transitionSize);
        

        Graphics.Blit(src, dest, foveatedRenderingMaterial);
    }
}
