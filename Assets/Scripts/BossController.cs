using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class BossController : MonoBehaviour
{
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
    private AnimatorStateInfo currentAnimationState;
    private Rigidbody bossRB;
    private bool isAttacking = false;
    private bool hasSpawnedLavalBall;
    private bool waitForNextAttack = false;
    private int maxHealth = 1000;
    [SerializeField] private int currentHealth = 1000;
    [SerializeField] private float cameraMoveDuration = 1f;
    private float timeSinceLastAttack = 0f;
    [SerializeField] private float timeBetweenAttacks = 5f;

    private void Awake()
    {
        bossAnimation = GetComponent<Animator>();
        bossNavMesh = GetComponent<NavMeshAgent>();
        bossRB = GetComponent<Rigidbody>();
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();

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
        BossMovement();
        AttackCoolDown();
    }

    private void FreezeBossVelocity()
    {
        if (bossRB != null)
        {
            bossRB.velocity = Vector3.zero;
            bossRB.angularVelocity = Vector3.zero;

        }
    }

    private void FixedUpdate()
    {
        FreezeBossVelocity();
    }

    private void BossMovement()
    {
        if (bossNavMesh.enabled)
        {
            bossNavMesh.destination = player.position;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (isAttacking) // Check if the boss is currently attacking
            {
                // Check if the attack animation has finished playing
                currentAnimationState = bossAnimation.GetCurrentAnimatorStateInfo(0);
                if (!currentAnimationState.IsName("Attack"))
                {
                    FinishAttack();
                }
                else
                {
                    LookTowardsPlayer();
                }
            }
            else if (distanceToPlayer <= bossNavMesh.stoppingDistance)
            {
                if (!waitForNextAttack)
                {
                    AttackPlayer();
                    waitForNextAttack = true;
                }

                else
                {
                    // Idle for a certain duration after attacking
                    bossAnimation.SetBool("isWalking", false);
                    bossAnimation.SetBool("isAttacking", false);

                    LookTowardsPlayer();
                }
            }
            else if (distanceToPlayer > bossNavMesh.stoppingDistance)
            {
                ChasePlayer();
            }
        }
    }

    private void FinishAttack()
    {
        isAttacking = false; // Reset the flag when the attack animation is finished
        bossNavMesh.isStopped = false; // Allow the boss to move again
        bossAnimation.SetBool("isAttacking", false);
    }

    private void LookTowardsPlayer()
    {
        // Rotate the boss towards the player's direction
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        bossNavMesh.isStopped = true; // Stop the boss from moving while attacking
    }

    private void AttackPlayer()
    {
        bossAnimation.SetBool("isWalking", false);
        bossAnimation.SetBool("isAttacking", true);

        // Set the flag to false at the beginning of the attack sequence
        waitForNextAttack = false;

        // Check if the boss has already initiated the attack sequence
        if (!hasSpawnedLavalBall)
        {
            StartCoroutine(PlayAttackAnimation());
        }

        bossAudio.PlayOneShot(bossRoarClip, 1f);
    }

    private IEnumerator PlayAttackAnimation()
    {
        // Spawn the boss weapon
        StartCoroutine(InstantiateBossWeapon());

        hasSpawnedLavalBall = true;

        yield return new WaitForSeconds(3);

        bossAnimation.SetBool("isAttacking", false);
        waitForNextAttack = true;

        hasSpawnedLavalBall = false;
    }

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

    private void ChasePlayer()
    {
        bossNavMesh.isStopped = false;
        bossAnimation.SetBool("isWalking", true);
        bossAnimation.SetBool("isAttacking", false);
        hasSpawnedLavalBall = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            Bullet playerBullet = other.GetComponent<Bullet>();

            currentHealth -= playerBullet.damagePerHit;
            hitImpact.Play();

            healthBar.SetHealth(currentHealth);

            StartCoroutine(BossHitDetect());
        }
    }

    private IEnumerator InstantiateBossWeapon()
    {
        if (playerController.isGameActive)
        {
            yield return new WaitForSeconds(1.4f);

            bossAudio.PlayOneShot(groundHitClip, 1f);

            yield return new WaitForSeconds(0.3f);

            // Instantiate boss weapon at bossWeaponSpawnPoint position with appropriate rotation
            bossWeapon = Instantiate(bossWeaponPrefab, bossWeaponSpawnPoint.position, Quaternion.identity);
            // Ensure the boss weapon follows the boss rotation
            bossWeapon.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            if (!playerController.isGameActive)
            {
                Destroy(bossWeapon);
            }
        }
    }

    private IEnumerator BossHitDetect()
    {
        if (currentHealth <= 0)
        {
            playerController.isGameActive = false;

            bossAudio.PlayOneShot(bossDeathClip, 1f);

            StartCoroutine(MovePlayerCameraToBoss());

            bossAnimation.SetTrigger("isDead");
            bossAnimation.SetBool("isWalking", false);
            bossAnimation.SetBool("isAttacking", false);
            bossNavMesh.enabled = false;
            gameObject.layer = 10;
            Destroy(bossWeapon);

            yield return new WaitForSeconds(3.5f);

            ParticleSystem instantiatedEffect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            instantiatedEffect.Play();

            yield return new WaitForSeconds(0.1f);

            gameObject.SetActive(false);

            gameWonText.gameObject.SetActive(true);
        }
    }

    private IEnumerator MovePlayerCameraToBoss()
    {
        // Calculate the target position for the camera
        Vector3 targetPosition = new Vector3(transform.position.x, playerCameraTransform.position.y, transform.position.z - 14);

        // Store the initial camera position
        Vector3 initialPosition = playerCameraTransform.position;

        // Duration for the camera movement
        float timeElapsed = 0f;

        // Smoothly move the camera to the boss position
        while (timeElapsed < cameraMoveDuration)
        {
            // Calculate the interpolation factor
            float t = timeElapsed / cameraMoveDuration;

            // Smoothly move towards the boss position
            playerCameraTransform.position = Vector3.Lerp(initialPosition, targetPosition, t);

            // Increment time
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the camera reaches the exact target position
        playerCameraTransform.position = targetPosition;
    }
}
