using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerTransform; // Reference to the player's transform
    public Vector3 playerOffset; // Offset from the player's position
    public float xBoundary = 45; // Boundary for the camera's x position
    public float positiveZBoundary = 40; // Boundary for the camera's positive z position
    public float negativeZBoundary = -80; // Boundary for the camera's negative z position
    private PlayerController player; // Reference to the PlayerController script

    private void Awake()
    {
        // Finding and getting reference to the PlayerController script
        player = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        // If the game is active, follow the player and enforce camera boundaries
        if (player.isGameActive)
        {
            // Set camera position to player's position plus offset
            transform.position = playerTransform.position + playerOffset;
            // Enforce camera boundaries
            CameraBoundary();
        }
    }

    // Method to enforce camera boundaries
    void CameraBoundary()
    {
        // Clamp camera's x and z positions within boundaries
        float clampedX = Mathf.Clamp(transform.position.x, -xBoundary, xBoundary);
        float clampedZ = Mathf.Clamp(transform.position.z, negativeZBoundary, positiveZBoundary);
        // Update camera's position with clamped values
        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }
}
