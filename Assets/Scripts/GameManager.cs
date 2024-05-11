using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject bossHealthBarImage;
    private PlayerController playerController;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject playerPowerUpPrefab;
    [SerializeField] private GameObject boss;
    public TMP_Text timerText;
    public TMP_Text enemyWaveText;
    private float timer = 30f;
    private float initialEnemySpeed = 7f;
    private int currentWave = 1;
    private bool spawningEnemies = false;
    private bool timerStarted = true;
    private bool powerUpInstantiated = false;
    private bool powerUpCoroutineRunning = false;


    private void Awake()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }


    // Start is called before the first frame update
    void Start()
    {
        if (playerController.isGameActive)
        {
            StartWave();
        }
    }


    // Update is called once per frame
    void Update()
    {
        // If the game is active, update the timer and check for power-up instantiation
        if (playerController.isGameActive)
        {
            CountDownTimer();

            // Check if a power-up should be instantiated and if a coroutine is not already running
            if (!powerUpInstantiated && !powerUpCoroutineRunning)
            {
                StartCoroutine(PowerUpInstantiate());
            }
        }

        // If the game is not active, stop spawning enemies
        else
        {
            spawningEnemies = false; // Stop spawning enemies
        }
    }

    // Method to handle the countdown timer
    private void CountDownTimer()
    {

        if (timerStarted)
        {
            // Ensure timer doesn't go below 0
            if (timer < 0)
            {
                timer = 0;
            }

            timer -= Time.deltaTime; // Decrease the timer by the time elapsed since the last frame
            timerText.text = Mathf.CeilToInt(timer).ToString(); // Update the timer text

            if (timer <= 0)
            {
                // Timer has ended, stop the timer
                timerStarted = false;
                Debug.Log("Timer ended.");
                CheckEndWave();
            }

            else if (spawningEnemies && timer % 2f <= Time.deltaTime)
            {
                // Spawn an enemy every 2 seconds
                SpawnEnemy();
            }
        }
    }

    // Method to start a new wave of Enemies
    private void StartWave()
    {
        timer = 30f;
        timerStarted = true;
        spawningEnemies = true;
        enemyWaveText.text = "Wave " + currentWave;

        if (currentWave == 5)
        {
            if (boss != null)
            {
                // Activate the boss and boss health bar for wave 5
                boss.SetActive(true);
                bossHealthBarImage.SetActive(true);
                timerStarted = false;
                timerText.gameObject.SetActive(false); // Hide timer text for wave 5
                StartCoroutine(SpawnEnemyWave5());
            }

            else
            {
                Debug.LogWarning("Boss GameObject not found!");
            }
        }
    }

    // Coroutine to spawn enemies for wave 5
    private IEnumerator SpawnEnemyWave5()
    {
        if (playerController.isGameActive)
        {
            while (true)
            {
                // Spawn an enemy every 3 seconds for wave 5
                SpawnEnemy();
                yield return new WaitForSeconds(3f); // Spawn enemies every 3 seconds for wave 5
            }
        }

    }

    // Method to check if the wave has ended
    void CheckEndWave()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            // If no enemies are left, start the next wave
            currentWave++;
            enemyWaveText.text = "Wave " + currentWave;
            if (currentWave <= 5)
            {
                StartWave();
            }
            else
            {
                Debug.Log("Game Over! All waves completed.");
            }
        }

        else
        {
            // If enemies are still alive, restart the timer and spawning
            timerStarted = true;
            spawningEnemies = true;
        }
    }

    // Method to spawn enemies
    void SpawnEnemy()
    {
        float xPos;
        float zPos;

        // Randomly choose between spawning at each corner
        int corner = Random.Range(0, 4);
        switch (corner)
        {
            case 0: // Right border
                xPos = 65f;
                zPos = Random.Range(-65f, 65f);
                break;
            case 1: // Left border
                xPos = -65f;
                zPos = Random.Range(-65f, 65f);
                break;
            case 2: // Top border
                xPos = Random.Range(-65f, 65f);
                zPos = 65f;
                break;
            case 3: // Bottom border
                xPos = Random.Range(-65f, 65f);
                zPos = -65f;
                break;
            default: // Shouldn't happen, default to right border
                xPos = 65f;
                zPos = Random.Range(-65f, 65f);
                break;
        }

        // Ensure enemies are only spawned if the timer is above 9 seconds and the game is active
        if (timer > 9 && playerController.isGameActive)
        {
            Vector3 enemyPosition = new Vector3(xPos, enemyPrefab.transform.position.y, zPos);
            GameObject newEnemy = Instantiate(enemyPrefab, enemyPosition, Quaternion.identity);

            NavMeshAgent enemyNavMesh = newEnemy.GetComponent<NavMeshAgent>();

            if (enemyNavMesh != null)
            {
                // Set the speed of enemies based on the current wave
                enemyNavMesh.speed = initialEnemySpeed + (currentWave - 1) * 0.5f;
            }
        }
    }

    // Coroutine to instantiate power-ups
    private IEnumerator PowerUpInstantiate()
    {
        float xPos;
        float zPos;

        // Randomly choose between spawning at each corner
        int corner = Random.Range(0, 4);
        switch (corner)
        {
            case 0: // Right border
                xPos = 20f;
                zPos = Random.Range(-20f, 20f);
                break;
            case 1: // Left border
                xPos = -20f;
                zPos = Random.Range(-20f, 20f);
                break;
            case 2: // Top border
                xPos = Random.Range(-20f, 20f);
                zPos = 20f;
                break;
            case 3: // Bottom border
                xPos = Random.Range(-20f, 20f);
                zPos = -20f;
                break;
            default: // Shouldn't happen, default to right border
                xPos = 20f;
                zPos = Random.Range(-20f, 20f);
                break;
        }

        // Instantiate power-up if not already instantiated and the game is active
        if (!powerUpInstantiated && playerController.isGameActive)
        {
            powerUpCoroutineRunning = true;
            yield return new WaitForSeconds(20);

            powerUpInstantiated = true;
            Vector3 powerUpPosition = new Vector3(xPos, transform.position.y, zPos);
            GameObject newPowerUp = Instantiate(playerPowerUpPrefab, powerUpPosition, Quaternion.identity);

            ParticleSystem powerUpParticle = newPowerUp.GetComponentInChildren<ParticleSystem>();

            if (powerUpParticle != null)
            {
                powerUpParticle.Play();
            }

            yield return new WaitForSeconds(10f);

            powerUpInstantiated = false;
            powerUpCoroutineRunning = false;
        }
    }

    // Method to restart the game
    public void RestartGame()
    {
        SceneManager.LoadScene(1);
    }

    // Method to return to the main menu
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
