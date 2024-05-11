using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private Transform chestTransform;
    private Transform playerTransform;
    public int maxHealth;
    public int currentHealth;
    private NavMeshAgent enemyNavMesh;
    private Rigidbody enemyRB;
    private Animator enemyAnimation;
    public int damagePerHit = 10;
    private bool isChasing;
    private PlayerController player;
    private ParticleSystem hitImpact;
    public AudioSource enemyAudio;
    public AudioClip deathClip;
    public AudioClip enemyWalkClip;

    private void Awake()
    {
        // Getting references to components and objects
        enemyRB = GetComponent<Rigidbody>();
        enemyNavMesh = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponentInChildren<Animator>();
        chestTransform = GameObject.Find("Chest").GetComponent<Transform>();
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        hitImpact = GetComponentInChildren<ParticleSystem>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartChasing();
    }

    // Update is called once per frame
    void Update()
    {
        // Update enemy movement
        UpdateMove();
    }

    private void FixedUpdate()
    {
        // Ensure enemy velocity is frozen
        FreezeVelocity();
    }

    // Start chasing either the player or the chest
    void StartChasing()
    {
        isChasing = true;
        enemyAnimation.SetBool("isWalking", true);
    }

    // Update enemy movement
    private void UpdateMove()
    {
        // If the game is active, update enemy movement
        if (player.isGameActive)
        {
            if (enemyNavMesh.enabled)
            {
                if (playerTransform != null && chestTransform != null)
                {
                    // Determine whether to chase the player or the chest
                    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                    float distanceToChest = Vector3.Distance(transform.position, chestTransform.position);

                    if (distanceToPlayer > distanceToChest)
                    {
                        enemyNavMesh.SetDestination(chestTransform.position);
                    }
                    else
                    {
                        enemyNavMesh.SetDestination(playerTransform.position);
                    }
                }
                // Stop or resume navigation based on whether enemy is chasing
                enemyNavMesh.isStopped = !isChasing;
            }
        }

        // If the game is not active, stop enemy movement
        else
        {
            enemyNavMesh.enabled = false;
            enemyAnimation.SetBool("isWalking", false);
        }
    }

    // Freeze enemy velocity
    private void FreezeVelocity()
    {
        if (isChasing && player.isGameActive)
        {
            enemyRB.angularVelocity = Vector3.zero;
            enemyRB.velocity = Vector3.zero;
        }
    }

    // Handle trigger collisions
    private void OnTriggerEnter(Collider other)
    {
        if (player.isGameActive)
        {
            // Handle collision with player's bullets
            if (other.gameObject.tag == "Bullet")
            {

                hitImpact.Play();
                Bullet bullet = other.GetComponent<Bullet>();
                currentHealth -= bullet.damagePerHit;
                Debug.Log("Current Health: " + currentHealth);

                Vector3 reactPosition = transform.position - other.transform.position;
                Destroy(other.gameObject);

                StartCoroutine(DetectEnemyHitRoutine());
            }

            // Handle collision with the chest or shield
            if (other.gameObject.tag == "Chest")
            {
                Debug.Log("Collided with Chest");
                enemyAnimation.SetTrigger("isAttacking");
                isChasing = false;
                gameObject.layer = 10;

                StartCoroutine(DeathDelay());

                Destroy(gameObject, 1.2f);

            }

            else if (other.gameObject.CompareTag("Shield"))
            {
                enemyAnimation.SetTrigger("isAttacking");
                isChasing = false;
                gameObject.layer = 10;

                StartCoroutine(DeathDelay());

                Destroy(gameObject, 1.2f);
            }
        }

    }

    // Handle collision with the player
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            enemyNavMesh.enabled = false;
            gameObject.layer = 10;
            enemyAnimation.SetTrigger("isAttacking");

            StartCoroutine(DeathDelay());

            Destroy(gameObject, 1.2f);
        }
    }

    // Delay enemy death animation and sound
    private IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(0.35f);

        enemyAudio.PlayOneShot(deathClip, 0.3f);
    }

    // Detect if the enemy is hit and update its state accordingly
    private IEnumerator DetectEnemyHitRoutine()
    {
        if (currentHealth <= 0)
        {
            gameObject.layer = 10;
            enemyAnimation.SetTrigger("isDead");
            enemyNavMesh.enabled = false;

            enemyAudio.PlayOneShot(deathClip, 0.5f);

            Destroy(gameObject, 2);
        }


        yield return null;
    }
}
