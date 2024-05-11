using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    // Serialized fields for inspector visibility
    public TMP_Text gameWonText;
    [SerializeField] private AudioClip bossRoarClip;
    [SerializeField] private AudioClip groundHitClip;
    [SerializeField] private AudioSource bossAudio;
    [SerializeField] private AudioClip bossDeathClip;
    private GameObject bossWeapon;
    [SerializeField] private ParticleSystem hitImpact;
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private GameObject bossWeaponPrefab; // Reference to the boss weapon prefab
    [SerializeField] private Transform bossWeaponSpawnPoint; // Spawn point for the boss weapon
    private PlayerController playerController;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerCameraTransform;
    private NavMeshAgent bossNavMesh;
    private Animator bossAnimation;
    private Rigidbody bossRB;
    private bool isAttacking = false;
    private bool hasSpawnedLavalBall;
    private bool waitForNextAttack = false;
    [SerializeField] private int maxHealth = 1000;
    [SerializeField] private int currentHealth = 1000;
    [SerializeField] private float cameraMoveDuration = 1f;
    private float timeSinceLastAttack = 0f;
    [SerializeField] private float timeBetweenAttacks = 5f;

    private void Awake()
    {
        // Get references to components
        bossAnimation = GetComponent<Animator>();
        bossNavMesh = GetComponent<NavMeshAgent>();
        bossRB = GetComponent<Rigidbody>();
        playerController = FindObjectOfType<PlayerController>(); // Use FindObjectOfType instead of GameObject.Find
        currentHealth = maxHealth; // Initialize currentHealth here
        healthBar.SetMaxHealth(maxHealth);

    }
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        // If game is active, handle boss movement and attack cooldown
        if (playerController.isGameActive)
        {
            BossMovement();
            AttackCoolDown();
        }

        else
        {
            GameOver();
        }
    }

    private void FixedUpdate()
    {
        // Freeze boss velocity
        FreezeBossVelocity();
    }

    // Method to freeze boss velocity
    private void FreezeBossVelocity()
    {
        if (bossRB != null)
        {
            bossRB.velocity = Vector3.zero;
            bossRB.angularVelocity = Vector3.zero;
        }
    }

    // Method to handle boss movement
    private void BossMovement()
    {
        if (playerController.isGameActive)
        {
            // If boss is not attacking and game is active
            if (bossNavMesh.enabled && !isAttacking) // Add condition to check if not attacking
            {
                // Set destination to player position
                bossNavMesh.destination = player.position;

                // Calculate distance to player
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                // If distance to player is within stopping distance
                if (distanceToPlayer <= bossNavMesh.stoppingDistance)
                {
                    // If not waiting for next attack, initiate attack
                    if (!waitForNextAttack)
                    {
                        AttackPlayer();
                        waitForNextAttack = true;
                    }
                    else
                    {
                        // Stop walking animation and look towards player
                        bossAnimation.SetBool("isWalking", false);
                        bossAnimation.SetBool("isAttacking", false);
                        LookTowardsPlayer();
                    }
                }

                // If distance to player is greater than stopping distance, chase player
                else if (distanceToPlayer > bossNavMesh.stoppingDistance)
                {
                    ChasePlayer();
                }
            }
        }
    }

    // Method to make boss look towards player
    private void LookTowardsPlayer()
    {
        // Rotate the boss towards the player's direction
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        bossNavMesh.isStopped = true; // Stop the boss from moving while attacking
    }

    // Method to initiate boss attack
    private void AttackPlayer()
    {
        if (playerController.isGameActive)
        {
            bossAnimation.SetBool("isWalking", false);
            bossAnimation.SetBool("isAttacking", true);
            waitForNextAttack = false;
            if (!hasSpawnedLavalBall)
            {
                StartCoroutine(PlayAttackAnimation());
            }
            bossAudio.PlayOneShot(bossRoarClip, 1f);
        }

    }

    // Coroutine to play boss attack animation
    private IEnumerator PlayAttackAnimation()
    {
        StartCoroutine(InstantiateBossWeapon());
        hasSpawnedLavalBall = true;
        yield return new WaitForSeconds(3);
        bossAnimation.SetBool("isAttacking", false);
        waitForNextAttack = true;
        hasSpawnedLavalBall = false;
    }

    // Method to handle attack cooldown
    private void AttackCoolDown()
    {
        if (waitForNextAttack)
        {
            timeSinceLastAttack += Time.deltaTime;
            if (timeSinceLastAttack >= timeBetweenAttacks)
            {
                waitForNextAttack = false;
                timeSinceLastAttack = 0f;
            }
        }
    }

    // Method to chase the player
    private void ChasePlayer()
    {
        if (playerController.isGameActive)
        {
            bossNavMesh.isStopped = false;
            bossAnimation.SetBool("isWalking", true);
            bossAnimation.SetBool("isAttacking", false);
            hasSpawnedLavalBall = false;
        }
    }

    // OnTriggerEnter is called when the Collider other enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (playerController.isGameActive)
        {
            if (other.gameObject.CompareTag("Bullet"))
            {
                // If collided object is bullet, reduce boss health and play hit impact
                Bullet playerBullet = other.GetComponent<Bullet>();
                if (playerBullet != null)
                {
                    currentHealth -= playerBullet.damagePerHit;
                    hitImpact.Play();
                    healthBar.SetHealth(currentHealth);
                    StartCoroutine(BossHitDetect());
                }
            }
        }
    }

    // Coroutine to instantiate boss weapon
    private IEnumerator InstantiateBossWeapon()
    {
        if (playerController != null && playerController.isGameActive)
        {
            // Delay before spawning boss weapon
            yield return new WaitForSeconds(1.4f);
            bossAudio.PlayOneShot(groundHitClip, 1f);
            yield return new WaitForSeconds(0.3f);

            // Instantiate boss weapon at spawn point
            bossWeapon = Instantiate(bossWeaponPrefab, bossWeaponSpawnPoint.position, Quaternion.identity);
            bossWeapon.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            if (!playerController.isGameActive)
            {
                Destroy(bossWeapon);
            }
        }
    }

    // Coroutine to handle boss hit detection
    private IEnumerator BossHitDetect()
    {
        if (currentHealth <= 0)
        {
            playerController.isGameActive = false;
            bossAudio.PlayOneShot(bossDeathClip, 1f);
            StartCoroutine(MovePlayerCameraToBoss());

            // Trigger boss death animation
            bossAnimation.SetTrigger("isDead");
            bossAnimation.SetBool("isWalking", false);
            bossAnimation.SetBool("isAttacking", false);
            bossNavMesh.enabled = false;

            // Change boss layer
            gameObject.layer = 10;
            Destroy(bossWeapon);
            yield return new WaitForSeconds(3.5f);

            // Instantiate death effect
            ParticleSystem instantiatedEffect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            instantiatedEffect.Play();
            yield return new WaitForSeconds(0.1f);
            gameObject.SetActive(false);
            gameWonText.gameObject.SetActive(true);

            // Show restart and return to menu buttons
            playerController.restartButton.gameObject.SetActive(true);
            playerController.returnToMenuButton.gameObject.SetActive(true);
        }
    }

    // Coroutine to move player camera to boss position
    private IEnumerator MovePlayerCameraToBoss()
    {
        Vector3 targetPosition = new Vector3(transform.position.x, playerCameraTransform.position.y, transform.position.z - 14);
        Vector3 initialPosition = playerCameraTransform.position;
        float timeElapsed = 0f;
        while (timeElapsed < cameraMoveDuration)
        {
            float t = timeElapsed / cameraMoveDuration;
            playerCameraTransform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        playerCameraTransform.position = targetPosition;
    }

    // Method to handle game over state
    private void GameOver()
    {
        playerController.isGameActive = false;
        bossAnimation.SetBool("isWalking", false);
        bossAnimation.SetBool("isAttacking", false);
        bossNavMesh.enabled = false;
    }
}
