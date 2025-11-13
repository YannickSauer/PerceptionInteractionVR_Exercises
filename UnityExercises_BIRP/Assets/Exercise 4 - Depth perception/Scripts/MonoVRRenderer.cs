using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Camera))]
public class MonoVRRenderer : MonoBehaviour
{
    private Camera cam;
    public bool active = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
        // Center camera on the midpoint between eyes
        cam.stereoTargetEye = StereoTargetEyeMask.Both; // still render to VR
    }

    void OnPreCull()
    {
        if (!active)
            return;
        // get the view matrix that centers between the two eyes
        Matrix4x4 viewMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1)) * cam.transform.localToWorldMatrix.inverse; //
        // overwrite both eye view matrices with centered view
        cam.SetStereoViewMatrix(Camera.StereoscopicEye.Left, viewMatrix);
        cam.SetStereoViewMatrix(Camera.StereoscopicEye.Right, viewMatrix);
        cam.worldToCameraMatrix = viewMatrix;
    }

    void OnPostRender()
    {
        // Reset matrices
        cam.ResetStereoProjectionMatrices();
        cam.ResetStereoViewMatrices();
    }
}
