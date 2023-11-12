using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The target the camera follows (Player)
    public float smoothing = 100f; // The speed with which the camera will follow

    private Vector3 offset; // The initial offset from the target

    void Start()
    {
        // Calculate the initial offset.
        offset = transform.position - target.position;
    }

    void FixedUpdate()
    {
        // Create a position for the camera to aim at, based on the offset from the target.
        Vector3 targetCamPos = target.position + offset;

        // Smoothly interpolate between the camera's current position and its target position.
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
    }
}
