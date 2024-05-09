using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public TMP_Text gameOverText;
    public AudioClip bossBulletHitClip;
    public AudioSource playerAudio;
    public AudioClip playerDamageClip;
    public AudioClip gameOverClip;
    public GameObject shield;
    public ParticleSystem hitImpact;
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
    public bool isGameOver = false;
    public bool hasPowerUp = false;
    private List<Enemy> hitEnemies = new List<Enemy>();



    private void Awake()
    {
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
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
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

    private void FreezeRotation()
    {
        playerRB.angularVelocity = Vector3.zero;
        playerRB.velocity = Vector3.zero;
    }


    private void FixedUpdate()
    {
        if (isGameActive)
        {
            FreezeRotation();
        }

    }

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

    void PlayerMovement()
    {
        if (!isHit && isGameActive)
        {
            moveVector = new Vector3(horizontaInput, 0, verticalInput).normalized;

            if (isDodge)
                moveVector = dodgeVector;

            if (!isFireReady)
            {
                moveVector = Vector3.zero;
            }

            transform.position += moveVector * moveSpeed * Time.deltaTime;

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

            playerAnimation.SetBool("isRunning", moveVector != Vector3.zero);
        }

        else
        {
            playerAnimation.SetBool("isRunning", false);
        }
    }

    void PlayerTurnAtPosition()
    {
        transform.LookAt(transform.position + moveVector);
    }

    void PlayerBoundary()
    {
        float clampedX = Mathf.Clamp(transform.position.x, -boundaryLimit, boundaryLimit);
        float clampedZ = Mathf.Clamp(transform.position.z, -boundaryLimit, boundaryLimit);

        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }

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

    private void OnTriggerEnter(Collider other)
    {
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

        else if (other.gameObject.CompareTag("PowerUp"))
        {
            Destroy(other.gameObject);
            hasPowerUp = true;

            StartCoroutine(ActivatePowerUp());
        }
    }

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

    private void PlayerDeath()
    {
        isGameActive = false;
        playerAudio.PlayOneShot(gameOverClip, 1f);
        chestManager.OpenChest();
        playerAnimation.SetTrigger("isDead");
        gameObject.layer = 10;
        StartCoroutine(delaySetActive());

    }

    private IEnumerator delaySetActive()
    {
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);

        gameOverText.gameObject.SetActive(true);
    }


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

    void PlayerDodgeOut()
    {
        moveSpeed *= 0.5f;
        isDodge = false;
    }
}
