using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRB;
    private WeaponController weaponController;
    private float horizontaInput;
    private float verticalInput;
    private float fireDelay;
    [SerializeField] float moveSpeed = 15f;
    private Vector3 moveVector;
    private Vector3 dodgeVector;
    private Animator playerAnimation;
    private bool dodgeButton;
    private bool isDodge;
    private bool fireButton;
    private bool isFireReady;
    private bool isBorder;


    private void Awake()
    {
        playerAnimation = GetComponentInChildren<Animator>();
        weaponController = GetComponentInChildren<WeaponController>();
        playerRB = GetComponentInChildren<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetPlayerInput();
        PlayerMovement();
        PlayerTurnAtPosition();
        PlayerAttack();
        PlayerDodgeIn();

    }

    private void FreezeRotation()
    {
        playerRB.angularVelocity = Vector3.zero;
    }

    private void DetectPlayerBoundary()
    {
        Debug.DrawRay(transform.position, transform.forward * 2.5f, Color.blue);

        isBorder = Physics.Raycast(transform.position, transform.forward, 2.5f, LayerMask.GetMask("Wall"));
    }

    private void FixedUpdate()
    {
        FreezeRotation();
        DetectPlayerBoundary();
    }

    void GetPlayerInput()

    {
        horizontaInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        dodgeButton = Input.GetButtonDown("Jump");
        fireButton = Input.GetButton("Fire1");
    }

    void PlayerMovement()
    {
        moveVector = new Vector3(horizontaInput, 0, verticalInput).normalized;

        if (isDodge)
            moveVector = dodgeVector;

        if (!isFireReady)
            moveVector = Vector3.zero;

        if (!isBorder)
            transform.position += moveVector * moveSpeed * Time.deltaTime;

        playerAnimation.SetBool("isRunning", moveVector != Vector3.zero);
    }

    void PlayerTurnAtPosition()
    {
        transform.LookAt(transform.position + moveVector);
    }


    void PlayerAttack()
    {
        fireDelay += Time.deltaTime;
        isFireReady = weaponController.bulletSpeedRate < fireDelay;

        if (fireButton && isFireReady && !isDodge)
        {
            weaponController.UseBullet();
            playerAnimation.SetTrigger("doShoot");
            fireDelay = 0;
        }
    }

    void PlayerDodgeIn()
    {
        if (dodgeButton && moveVector != Vector3.zero && !isDodge)
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
