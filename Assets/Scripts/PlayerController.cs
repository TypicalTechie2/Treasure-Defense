using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // References to UI elements and audio clips
    public Button returnToMenuButton;
    public Button restartButton;
    public TMP_Text gameOverText;
    public AudioClip bossBulletHitClip;
    public AudioSource playerAudio;
    public AudioClip playerDamageClip;
    public AudioClip gameOverClip;
    public GameObject shield;
    public ParticleSystem hitImpact;

    // Private variables for player control and stats
    private Rigidbody playerRB;
    private MeshRenderer[] playerMesh;
    private WeaponController weaponController;
    [SerializeField] private HealthBar healthBar;
    private ChestManager chestManager;
    private float horizontaInput;
    private float verticalInput;
    private float fireDelay;
    private float boundaryLimit = 70f;
    [SerializeField] float moveSpeed = 12f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    private Vector3 moveVector;
    private Vector3 dodgeVector;
    private Animator playerAnimation;
    private bool dodgeButton;
    private bool isDodge;
    private bool fireButton;
    private bool isFireReady;
    private bool isHit;
    public bool isGameActive = true;
    public bool hasPowerUp = false;
    private List<Enemy> hitEnemies = new List<Enemy>();



    private void Awake()
    {
        // Getting references to components and objects
        playerAnimation = GetComponentInChildren<Animator>();
        weaponController = GetComponentInChildren<WeaponController>();
        playerRB = GetComponentInChildren<Rigidbody>();
        playerMesh = GetComponentsInChildren<MeshRenderer>();

        GameObject chestObject = GameObject.Find("Chest");
        if (chestObject != null)
        {
            chestManager = chestObject.GetComponent<ChestManager>();
            if (chestManager == null)
            {
                Debug.LogError("ChestManager component not found on the Chest GameObject.");
            }
        }
        else
        {
            Debug.LogError("Chest GameObject not found.");
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        // Initializing player health and UI
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        // Handling player input and actions if the game is active
        if (isGameActive || !chestManager.isChestOpen)
        {
            GetPlayerInput();
            PlayerMovement();
            PlayerTurnAtPosition();
            PlayerAttack();
            PlayerDodgeIn();
            PlayerBoundary();
        }
    }
    private void FixedUpdate()
    {
        // Ensuring player rigidbody doesn't rotate unexpectedly
        if (isGameActive)
        {
            FreezeRotation();
        }
    }

    private void FreezeRotation()
    {

        playerRB.angularVelocity = Vector3.zero;
        playerRB.velocity = Vector3.zero;
    }

    // Getting player input
    void GetPlayerInput()
    {
        if (isGameActive)
        {
            horizontaInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            dodgeButton = Input.GetButtonDown("Jump");
            fireButton = Input.GetButton("Fire1");
        }

    }

    // Player movement handling
    void PlayerMovement()
    {
        if (!isHit && isGameActive)
        {
            // Calculating movement vector
            moveVector = new Vector3(horizontaInput, 0, verticalInput).normalized;

            if (isDodge)
                moveVector = dodgeVector;

            if (!isFireReady)
            {
                moveVector = Vector3.zero;
            }

            // Moving the player
            transform.position += moveVector * moveSpeed * Time.deltaTime;

            // Turning the player towards movement direction
            if (Input.GetMouseButton(0))
            {
                // Get the position of the mouse click in the world space
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = Camera.main.transform.position.y - transform.position.y;
                Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);

                // Calculate the direction vector from player to target position
                Vector3 direction = (targetPosition - transform.position).normalized;

                // Calculate the rotation only around the y-axis and set it directly
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            }

            // Triggering run animation
            playerAnimation.SetBool("isRunning", moveVector != Vector3.zero);
        }

        else
        {
            playerAnimation.SetBool("isRunning", false);
        }
    }

    // Turning the player towards movement direction
    void PlayerTurnAtPosition()
    {
        transform.LookAt(transform.position + moveVector);
    }

    // Restricting player movement within boundaries
    void PlayerBoundary()
    {
        float clampedX = Mathf.Clamp(transform.position.x, -boundaryLimit, boundaryLimit);
        float clampedZ = Mathf.Clamp(transform.position.z, -boundaryLimit, boundaryLimit);

        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }

    // Handling collision with enemies
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy") && currentHealth > 0)
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();

            // Check if the enemy is already in the list of hit enemies
            if (!hitEnemies.Contains(enemy))
            {
                // Apply damage to the player
                currentHealth -= enemy.damagePerHit;
                healthBar.SetHealth(currentHealth);
                playerAudio.PlayOneShot(playerDamageClip, 1f);

                // Optionally, provide visual feedback here
                StartCoroutine(PlayerDamage());

                // Add the enemy to the list
                hitEnemies.Add(enemy);
            }
        }
    }

    // Handling trigger collisions
    private void OnTriggerEnter(Collider other)
    {
        // Handling collision with enemy bullets
        if (other.gameObject.CompareTag("Enemy Bullet") && currentHealth > 0 && isGameActive)
        {
            hitImpact.Play();
            playerAudio.PlayOneShot(bossBulletHitClip, 1f);

            BossWeapon bossWeapon = other.gameObject.GetComponent<BossWeapon>();

            if (!hasPowerUp)
            {
                currentHealth -= bossWeapon.damagePerHit;
            }

            Destroy(other.gameObject);

            if (currentHealth <= 0)
            {
                PlayerDeath();
            }
        }

        // Handling collision with power-ups
        else if (other.gameObject.CompareTag("PowerUp"))
        {
            Destroy(other.gameObject);
            hasPowerUp = true;

            StartCoroutine(ActivatePowerUp());
        }
    }

    // Activating power-up effect
    private IEnumerator ActivatePowerUp()
    {
        hasPowerUp = true;
        shield.SetActive(true);
        gameObject.layer = 18;

        foreach (MeshRenderer meshRenderer in playerMesh)
        {
            meshRenderer.material.color = Color.green;
        }

        yield return new WaitForSeconds(5);

        hasPowerUp = false;
        shield.SetActive(false);
        gameObject.layer = 7;

        foreach (MeshRenderer meshRenderer in playerMesh)
        {
            Color customColor = new Color(0, 255, 127);
            meshRenderer.material.color = Color.white;
        }

    }

    // Handling player damage
    IEnumerator PlayerDamage()
    {
        isHit = true;

        foreach (MeshRenderer meshRenderer in playerMesh)
        {
            meshRenderer.material.color = Color.yellow;
        }
        yield return new WaitForSeconds(1f);

        hitEnemies.RemoveAt(0);

        if (hitEnemies.Count == 0)
        {
            isHit = false;

            foreach (MeshRenderer meshRenderer in playerMesh)
            {
                meshRenderer.material.color = Color.white;
            }
        }

        if (currentHealth <= 0)
        {
            PlayerDeath();
        }
    }

    // Handling player death
    private void PlayerDeath()
    {
        isGameActive = false;
        playerAudio.PlayOneShot(gameOverClip, 1f);
        chestManager.OpenChest();
        playerAnimation.SetTrigger("isDead");
        gameObject.layer = 10;
        StartCoroutine(delaySetActive());

    }

    // Setting game objects inactive after delay
    private IEnumerator delaySetActive()
    {
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);

        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        returnToMenuButton.gameObject.SetActive(true);
    }


    // Player attack handling
    void PlayerAttack()
    {
        fireDelay += Time.deltaTime;
        isFireReady = weaponController.bulletSpeedRate < fireDelay;

        if (fireButton && isFireReady && !isDodge && !isHit && isGameActive)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.transform.position.y - transform.position.y;

            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 targetPosition = hit.point;
                Vector3 direction = (targetPosition - transform.position).normalized;
                direction.y = 0;

                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

                weaponController.UseBullet(direction);
                playerAnimation.SetTrigger("doShoot");
                fireDelay = 0;
            }
        }
    }

    // Player dodge handling
    void PlayerDodgeIn()
    {
        if (dodgeButton && moveVector != Vector3.zero && !isDodge && !isHit)
        {
            dodgeVector = moveVector;
            moveSpeed *= 2;
            playerAnimation.SetTrigger("doDodge");
            isDodge = true;

            Invoke("PlayerDodgeOut", 0.5f);
        }
    }

    // Resetting dodge effects
    void PlayerDodgeOut()
    {
        moveSpeed *= 0.5f;
        isDodge = false;
    }
}
