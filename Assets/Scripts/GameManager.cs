using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
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
        if (playerController.isGameActive)
        {
            CountDownTimer();

            // Check if a power-up should be instantiated and if a coroutine is not already running
            if (!powerUpInstantiated && !powerUpCoroutineRunning)
            {
                StartCoroutine(PowerUpInstantiate());
            }
        }
    }

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
                SpawnEnemy();
            }
        }
    }

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
                boss.SetActive(true);
            }

            else
            {
                Debug.LogWarning("Boss GameObject not found!");
            }
        }
    }

    void CheckEndWave()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
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
            timerStarted = true;
            spawningEnemies = true;
        }
    }

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

        if (timer > 9)
        {
            Vector3 enemyPosition = new Vector3(xPos, enemyPrefab.transform.position.y, zPos);
            GameObject newEnemy = Instantiate(enemyPrefab, enemyPosition, Quaternion.identity);

            NavMeshAgent enemyNavMesh = newEnemy.GetComponent<NavMeshAgent>();

            if (enemyNavMesh != null)
            {
                enemyNavMesh.speed = initialEnemySpeed + (currentWave - 1) * 0.5f;
            }
        }
    }

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

        if (!powerUpInstantiated)
        {
            powerUpCoroutineRunning = true;
            yield return new WaitForSeconds(10f);

            powerUpInstantiated = true;
            Vector3 powerUpPosition = new Vector3(xPos, transform.position.y, zPos);
            Instantiate(playerPowerUpPrefab, powerUpPosition, Quaternion.identity);


            yield return new WaitForSeconds(10f);

            powerUpInstantiated = false;
            powerUpCoroutineRunning = false;
        }
    }
}
