using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChestManager : MonoBehaviour
{
    public int ChestMaxHealth = 100;
    public int ChestCurrentHealth = 100;
    private Vector3 originalPosition;
    public float vibrateMagnitude = 0.1f;
    public float vibrateDuration = 0.3f;
    public float cameraMoveDuration = 5f;
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

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy" && ChestCurrentHealth > 0)
        {
            yield return new WaitForSeconds(0.6f);

            StartCoroutine(VibrateChest());

            Enemy enemy = other.GetComponent<Enemy>();

            ChestCurrentHealth -= enemy.damagePerHit;
            healthBar.SetHealth(ChestCurrentHealth);

            Debug.Log("Current Health: " + ChestCurrentHealth);

            if (ChestCurrentHealth <= 0 && !isChestOpen)
            {
                OpenChest();
            }
        }
    }

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

    private IEnumerator MovePlayerCameraToChest()
    {
        Vector3 targetPosition = new Vector3(0, 20, -10);

        Vector3 initialPosition = playerCameraTransform.position;
        float timeElapsed = 0f;

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

        yield return new WaitForSeconds(3);

        playerController.gameOverText.gameObject.SetActive(true);
    }


    private IEnumerator ChestVibrate()
    {
        Quaternion targetRotation = Quaternion.Euler(-90f, 0f, -180f);
        float duration = 1f; // Adjust as needed

        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            openChest.transform.rotation = Quaternion.Slerp(openChest.transform.rotation, targetRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        openChest.transform.rotation = targetRotation;
    }

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
