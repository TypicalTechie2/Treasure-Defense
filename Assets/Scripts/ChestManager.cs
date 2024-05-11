using System.Collections;
using UnityEngine;

public class ChestManager : MonoBehaviour
{
    public int ChestMaxHealth = 100;
    public int ChestCurrentHealth = 100;
    private Vector3 originalPosition;
    public float vibrateMagnitude = 0.2f;
    public float vibrateDuration = 0.3f;
    public float cameraMoveDuration = 1f;
    public bool isChestOpen;
    public GameObject openChest;
    [SerializeField] GameObject crystal;
    public Transform playerCameraTransform; // Reference to the player camera transform
    private PlayerController playerController;
    private Animator playerAnimation;
    public GameObject chestLight;
    [SerializeField] private HealthBar healthBar;


    private void Awake()
    {
        originalPosition = transform.position;
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        playerAnimation = GameObject.Find("Player").GetComponentInChildren<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ChestCurrentHealth = ChestMaxHealth;
        healthBar.SetMaxHealth(ChestMaxHealth);
    }

    // OnTriggerEnter is called when the Collider other enters the trigger
    private IEnumerator OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to an enemy and chest health is greater than 0
        if (other.gameObject.tag == "Enemy" && ChestCurrentHealth > 0)
        {
            yield return new WaitForSeconds(0.6f); // Wait for a short delay before processing

            StartCoroutine(VibrateChest()); // Start vibrating the chest

            Enemy enemy = other.GetComponent<Enemy>();  // Get the Enemy component from the collider

            // Decrease chest health by enemy's damage
            ChestCurrentHealth -= enemy.damagePerHit;
            healthBar.SetHealth(ChestCurrentHealth);

            Debug.Log("Current Health: " + ChestCurrentHealth);

            // If chest health is depleted and the chest is not already open, open the chest
            if (ChestCurrentHealth <= 0 && !isChestOpen)
            {
                OpenChest();
            }
        }
    }

    // Coroutine to vibrate the chest
    private IEnumerator VibrateChest()
    {
        if (ChestCurrentHealth > 0)
        {
            float elapsedTime = 0;
            while (elapsedTime < vibrateDuration)
            {
                // Generate random offset for the chest's position
                float offsetX = Random.Range(-vibrateMagnitude, vibrateMagnitude);
                float offsetY = Random.Range(-vibrateMagnitude, vibrateMagnitude);
                float offsetZ = Random.Range(-vibrateMagnitude, vibrateMagnitude);

                // Apply the offset to the chest's position
                transform.position = originalPosition + new Vector3(offsetX, offsetY, offsetZ);

                // Increment elapsed time
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            transform.position = originalPosition;
        }
    }

    // Method to open the chest
    public void OpenChest()
    {
        isChestOpen = true;
        gameObject.layer = 10;
        playerController.isGameActive = false;
        playerAnimation.SetTrigger("isDead");
        playerController.playerAudio.PlayOneShot(playerController.gameOverClip, 1f);

        // Rotate the openChest object on the X-axis to -90
        StartCoroutine(ChestVibrate());

        // Move the crystal object up and rotate continuously
        if (crystal != null)
        {
            StartCoroutine(MoveAndRotateCrystal());
            chestLight.SetActive(true);
        }

        StartCoroutine(MovePlayerCameraToChest());
    }

    // Coroutine to move the player camera to the chest position
    private IEnumerator MovePlayerCameraToChest()
    {
        Vector3 targetPosition = new Vector3(0, 30, -17);

        Vector3 initialPosition = playerCameraTransform.position;
        float timeElapsed = 0f;

        // Interpolate camera position towards the chest position over time
        while (timeElapsed < cameraMoveDuration)
        {
            // Calculate the interpolation factor
            float t = timeElapsed / cameraMoveDuration;

            // Smoothly move towards the chest position
            playerCameraTransform.position = Vector3.Lerp(initialPosition, targetPosition, t);

            // Increment time
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the camera reaches the exact target position
        playerCameraTransform.position = targetPosition;

        yield return new WaitForSeconds(2);

        // Activate game over UI elements
        playerController.gameOverText.gameObject.SetActive(true);
        playerController.restartButton.gameObject.SetActive(true);
        playerController.returnToMenuButton.gameObject.SetActive(true);
    }

    // Method to vibrate the chest.
    private IEnumerator ChestVibrate()
    {
        Quaternion targetRotation = Quaternion.Euler(-90f, 0f, -180f);
        float duration = 1f;

        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            openChest.transform.rotation = Quaternion.Slerp(openChest.transform.rotation, targetRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        openChest.transform.rotation = targetRotation;
    }

    // Coroutine to move and rotate the crystal
    private IEnumerator MoveAndRotateCrystal()
    {
        Vector3 initialPosition = crystal.transform.position;

        float rotateSpeed = 20;

        while (true)
        {
            crystal.transform.position = initialPosition + Vector3.up * 1;
            crystal.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
