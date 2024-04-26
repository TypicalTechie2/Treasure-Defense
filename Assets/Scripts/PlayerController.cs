using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public WeaponController weaponController;
    private float horizontaInput;
    private float verticalInput;
    [SerializeField] float moveSpeed = 15f;
    private Vector3 moveVector;
    private Vector3 dodgeVector;
    private Animator playerAnimation;
    private bool dodgeButton;
    private bool isDodge;
    private bool fireButton;
    private bool isFiring;


    private void Awake()
    {
        playerAnimation = GetComponentInChildren<Animator>();
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

    void GetPlayerInput()

    {
        horizontaInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        dodgeButton = Input.GetButtonDown("Jump");
        fireButton = Input.GetButtonDown("Fire1");
    }

    void PlayerMovement()
    {
        if (!isFiring)
        {
            moveVector = new Vector3(horizontaInput, 0, verticalInput).normalized;

            if (isDodge)
                moveVector = dodgeVector;

            transform.position += moveVector * moveSpeed * Time.deltaTime;

            playerAnimation.SetBool("isRunning", moveVector != Vector3.zero);
        }

    }

    void PlayerTurnAtPosition()
    {
        transform.LookAt(transform.position + moveVector);
    }



    void PlayerDodgeIn()
    {
        if (dodgeButton && moveVector != Vector3.zero && !isDodge && !isFiring)
        {
            dodgeVector = moveVector;
            moveSpeed *= 2;
            playerAnimation.SetTrigger("doDodge");
            isDodge = true;

            Invoke("PlayerDodgeOut", 0.5f);
        }
    }

    void PlayerAttack()
    {
        if (fireButton && !isDodge)
        {
            weaponController.UseBullet();
            playerAnimation.SetTrigger("doShoot");
            isFiring = true;
            Invoke("StopFiring", 1f);
        }
    }

    void PlayerDodgeOut()
    {
        moveSpeed *= 0.5f;
        isDodge = false;
    }

    void StopFiring()
    {
        isFiring = false;
    }
}
