using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FetchBehavior : MonoBehaviour
{
    public GameObject targetObject;

    public GameObject player;

    public Transform snout;

    public BallFetchState ballFetchState;

    private Rigidbody targetRigidbody;

    public float movementSpeed = 2.0f;

    public float pickupDistance = 1.0f;

    public bool followPlayer = false;

    private void Start()
    {
        if (ballFetchState == null)
        {
            if(targetObject.GetComponent<BallFetchState>() != null)
                ballFetchState = targetObject.GetComponent<BallFetchState>();
            else
                Debug.LogWarning("BallFetchState component not found on targetObject.");
        }

        if (targetRigidbody == null && targetObject != null)
        {
            if (targetObject.GetComponent<Rigidbody>() != null)
                targetRigidbody = targetObject.GetComponent<Rigidbody>();
            else
                Debug.LogWarning("Rigidbody component not found on targetObject.");
        }
    }

    private void Update()
    {
        if (ballFetchState.readyToFetch == true)
        {
            MoveTowardsTarget();
        }
        if (followPlayer == true)
        {
            MoveTowardsPlayer();
        }

    }

    private void MoveTowardsTarget(Transform target)
    {
        float step = movementSpeed * Time.deltaTime;
        Vector3 targetPosition = new Vector3(target.position.x, this.transform.position.y, target.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        Vector3 direction = targetPosition - transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void MoveTowardsTarget()
    {
        if (targetObject != null)
        {
            float distance = Vector3.Distance(snout.transform.position, targetObject.transform.position);
            MoveTowardsTarget(targetObject.transform);
            if (distance <= pickupDistance)
            {
                PickUpObject();
            }
        }
    }

    private void PickUpObject()
    {
        if (ballFetchState != null)
        {
            ballFetchState.SetReadyToFetch(false);

            targetObject.transform.position = snout.position;

            targetObject.transform.SetParent(snout);

            targetRigidbody.isKinematic = true;

            followPlayer = true;
        }
    }

    private void MoveTowardsPlayer()
    {
        float distance = Vector3.Distance(new Vector3(player.transform.position.x, 0, player.transform.position.z), new Vector3(snout.position.x, 0, snout.position.z));

        if (distance > 2*pickupDistance)
            MoveTowardsTarget(player.transform);
        else
            DropObject();      
    }

    private void DropObject()
    {
        targetObject.transform.SetParent(null);
        targetRigidbody.isKinematic = false;

        followPlayer = false;
    }
}
