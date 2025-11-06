using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallFetchState : MonoBehaviour
{
    public bool readyToFetch = true;

    // Option 1:
    public void SetReadyToFetch(bool ready)
    {
        readyToFetch = ready;
    }

    // Option 2:
    public void ToggleReadyToFetchOn()
    {
        readyToFetch = true;
    }

    public void OnConnectedToServer()
    {
        readyToFetch = false;
    }
}

