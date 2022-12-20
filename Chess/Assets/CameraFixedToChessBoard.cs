using UnityEngine;
using System.Collections;

public class CameraTrackingScript : MonoBehaviour
{
  // The tracked object to follow
    public Transform trackedObject;

    // The camera to position
    public Camera trackingCamera;

    // The distance from the tracked object to keep the camera
    public float distance = 10.0f;

    void Update()
    {
        // Calculate the position for the camera
        Vector3 targetPosition = trackedObject.position + trackedObject.forward * distance;

        // Set the position of the camera
        trackingCamera.transform.position = targetPosition;

        // Make the camera look at the tracked object
        trackingCamera.transform.LookAt(trackedObject);
    }
}