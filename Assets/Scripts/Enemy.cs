using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private Transform chestTransform;
    private Transform playerTransform;
    public int maxHealth;
    public int currentHealth;
    private NavMeshAgent enemyNavMesh;
    private Rigidbody enemyRB;
    private BoxCollider enemyBoxCollider;
    private Animator enemyAnimation;
    public int damagePerHit = 10;
    private bool isChasing;
    private PlayerController player;
    private ParticleSystem hitImpact;

    private void Awake()
    {
        enemyRB = GetComponent<Rigidbody>();
        enemyBoxCollider = GetComponent<BoxCollider>();
        enemyNavMesh = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponentInChildren<Animator>();
        chestTransform = GameObject.Find("Chest").GetComponent<Transform>();
        playerTransform = GameObject.Find("Player").GetComponent<Transform>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        hitImpact = GetComponentInChildren<ParticleSystem>();

        StartChasing();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (enemyNavMesh.enabled)
        {
            if (playerTransform != null && chestTransform != null)
            {
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

            enemyNavMesh.isStopped = !isChasing;
        }

        if (!player.isGameActive)
        {
            enemyNavMesh.enabled = false;
            enemyAnimation.SetBool("isWalking", false);
        }
    }

    void StartChasing()
    {
        isChasing = true;
        enemyAnimation.SetBool("isWalking", true);
    }

    private void FreezeVelocity()
    {
        if (isChasing)
        {
            enemyRB.angularVelocity = Vector3.zero;
            enemyRB.velocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        FreezeVelocity();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bullet")
        {

            hitImpact.Play();
            Bullet bullet = other.GetComponent<Bullet>();
            currentHealth -= bullet.damagePerHit;
            Debug.Log("Current Health: " + currentHealth);

            Vector3 reactPosition = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(DetectEnemyHitRoutine(reactPosition));
        }

        if (other.gameObject.tag == "Chest")
        {
            Debug.Log("Collided with Chest");
            enemyAnimation.SetTrigger("isAttacking");
            isChasing = false;
            gameObject.layer = 10;
            Destroy(gameObject, 2);

        }

        else if (other.gameObject.CompareTag("Shield"))
        {
            enemyAnimation.SetTrigger("isAttacking");
            isChasing = false;
            gameObject.layer = 10;
            Destroy(gameObject, 2);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            enemyNavMesh.enabled = false;
            gameObject.layer = 10;
            enemyAnimation.SetTrigger("isAttacking");
            Destroy(gameObject, 1.5f);
        }
    }

    private IEnumerator DetectEnemyHitRoutine(Vector3 reactPosition)
    {
        if (currentHealth <= 0)
        {
            gameObject.layer = 10;
            enemyAnimation.SetTrigger("isDead");
            enemyNavMesh.enabled = false;
            reactPosition = reactPosition.normalized;
            reactPosition += Vector3.up;
            enemyRB.AddForce(reactPosition * 100, ForceMode.Impulse);
            Destroy(gameObject, 2);
        }


        yield return null;
    }
}
