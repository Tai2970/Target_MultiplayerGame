using UnityEngine;
using Unity.Netcode;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The player's tank
    public float smoothSpeed = 5f; // Speed of camera following
    public float minZoom = 5f; // Minimum zoom level
    public float maxZoom = 10f; // Maximum zoom level
    public float zoomSpeed = 2f; // Speed of zooming

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null) return; // If no player tank is assigned, do nothing

        // Smoothly follow the player's position
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Zoom In/Out with Mouse Scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }
}
