using UnityEngine;
using Unity.Netcode;

public class TankMovement : NetworkBehaviour
{
    public float maxSpeed = 10f;  // Maximum speed for movement
    public float acceleration = 5f; // Acceleration speed
    public float deceleration = 6f; // Deceleration when stopping
    public float rotateSpeed = 200f; // Rotation speed

    private string horizontalInput, verticalInput;
    private bool controlsDisabled = false; // Flag to disable movement

    private float currentSpeed = 0f; // Store current movement speed
    private NetworkVariable<Vector3> tankPosition = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> tankRotation = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    private void Start()
    {
        if (!IsOwner) return; 

        // Assign movement controls based on player ID
        if (OwnerClientId == 0) // Player 1 (Host)
        {
            horizontalInput = "Horizontal";  // Uses "Horizontal" (A/D for rotation)
            verticalInput = "Vertical";      // Uses "Vertical" (W/S for forward/backward)
        }
        else // Player 2 (Client)
        {
            horizontalInput = "Player2_Horizontal";  // Uses "Player2_Horizontal" (Left/Right Arrows)
            verticalInput = "Player2_Vertical";      // Uses "Player2_Vertical" (Up/Down Arrows)
        }

        // Assign the camera to follow this player's tank
        if (IsOwner && Camera.main != null)
        {
            Camera.main.GetComponent<CameraFollow>().target = transform;
        }
    }

    private void Update()
    {
        if (IsOwner && !controlsDisabled) // Prevent movement when controls are disabled
        {
            float moveInput = Input.GetAxis(verticalInput); // Get forward/backward input (-1 to 1)
            float rotateInput = Input.GetAxis(horizontalInput); // Get left/right rotation input

            // Apply acceleration only when pressing W/S or Up/Down Arrow
            if (moveInput != 0)
            {
                currentSpeed += moveInput * acceleration * Time.deltaTime;
            }
            else
            {
                currentSpeed -= Mathf.Sign(currentSpeed) * deceleration * Time.deltaTime; // Slow down smoothly
            }

            // Clamp speed within range (-maxSpeed for reverse, +maxSpeed for forward)
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

            // Apply movement (move in the correct direction)
            Vector3 newPosition = transform.position + transform.up * currentSpeed * Time.deltaTime;

            // Apply rotation
            float newRotation = transform.rotation.eulerAngles.z - (rotateInput * rotateSpeed * Time.deltaTime);

            // Update network variables to sync across all players
            tankPosition.Value = newPosition;
            tankRotation.Value = newRotation;

            // Play movement sound if moving
            bool isMoving = Mathf.Abs(currentSpeed) > 0.1f;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTankMoveSound(isMoving);
            }
        }

        // Apply the synced position and rotation for all players
        transform.position = tankPosition.Value;
        transform.rotation = Quaternion.Euler(0, 0, tankRotation.Value);
    }

    // New method to disable movement when the game ends
    public void DisableControls()
    {
        controlsDisabled = true;

        // Stop tank movement sound when controls are disabled
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTankMoveSound(false);
        }
    }
}
