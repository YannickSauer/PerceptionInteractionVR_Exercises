using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawnerBehavior : MonoBehaviour
{
    public GameObject ball;

    public Rigidbody ballRigidbody;

    private void Start()
    {
        // try to get the rigidbody of the ball
        if (ball.GetComponent<Rigidbody>() == null)
        {
            Debug.Log("No Rigidbody component found on ball.");
        }
        else
        {
            ballRigidbody = ball.GetComponent<Rigidbody>();
        }
    }

    public void SpawnBall()
    {
        if (ball == null)
        {
            Debug.LogWarning("Ball GameObject is not assigned.");
        }
        else
        {
            ball.transform.position = this.transform.position + Vector3.up * 0.5f;
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }
    }
}
