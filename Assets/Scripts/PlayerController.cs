using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject shield;
    public ParticleSystem hitImpact;
    private Rigidbody playerRB;
    private MeshRenderer[] playerMesh;
    private WeaponController weaponController;
    private ChestManager chestManager;
    private float horizontaInput;
    private float verticalInput;
    private float fireDelay;
    private float boundaryLimit = 70f;
    [SerializeField] float moveSpeed = 12f;
    [SerializeField] private int playerHealth = 40;
    private Vector3 moveVector;
    private Vector3 dodgeVector;
    private Animator playerAnimation;
    private bool dodgeButton;
    private bool isDodge;
    private bool fireButton;
    private bool isFireReady;
    private bool isColliding;
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
        chestManager = GameObject.Find("Chest").GetComponent<ChestManager>();

    }
    // Start is called before the first frame update
    void Start()
    {

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
            // AvoidPassingThrough();
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

            if (!isColliding)
            {
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
        if (other.gameObject.CompareTag("Enemy") && playerHealth > 0)
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();

            // Check if the enemy is already in the list of hit enemies
            if (!hitEnemies.Contains(enemy))
            {
                // Apply damage to the player
                playerHealth -= enemy.damagePerHit;

                // Optionally, provide visual feedback here
                StartCoroutine(PlayerDamage());

                // Add the enemy to the list
                hitEnemies.Add(enemy);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy Bullet") && playerHealth > 0)
        {
            hitImpact.Play();

            BossWeapon bossWeapon = other.gameObject.GetComponent<BossWeapon>();

            if (!hasPowerUp)
            {
                playerHealth -= bossWeapon.damagePerHit;
            }

            Destroy(other.gameObject);

            if (playerHealth <= 0)
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

        if (playerHealth <= 0)
        {
            PlayerDeath();
        }
    }

    private void PlayerDeath()
    {
        isGameActive = false;
        chestManager.OpenChest();
        playerAnimation.SetTrigger("isDead");
        gameObject.layer = 10;
        StartCoroutine(delaySetActive());
    }

    private IEnumerator delaySetActive()
    {
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
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
        if (dodgeButton && moveVector != Vector3.zero && !isDodge && !isHit && !isColliding)
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
