using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement_scr : MonoBehaviour
{
    [Header("Parameters")]
    public CharacterController characterController;
    public Camera mainCamera;

    [Header("Movement Parameters")]
    //These floats handle all the movement speeds
    [SerializeField]
    private float crouchSpeed = 2;
    [SerializeField]
    private float walkSpeed = 4;
    [SerializeField]
    private float runSpeed = 6;

    [SerializeField]
    [Range(0.0f, 0.5f)]
    private float moveSmoothSpeed = 0.3f;

    [Header("Movement Type Check")]
    public bool isWalking;
    public bool isRunning;

    //These floats are used for the FOV lerp
    [SerializeField][Range(40.0f, 65.0f)] private float defaultFOV = 60f;
    private float maxRunFOV = 70f;
    public float currentFOV;
    public float fovMult = 0.2f;

    //Vectors to be used for SmoothDamp
    //These are for movement
    Vector2 currentDir = Vector2.zero;
    Vector2 currentDirVelocity = Vector2.zero;

    //These bools are used to check our players stance and movement
    [SerializeField] private bool canJump = true;

    [Header("Gravity Parameters")]
    [SerializeField] private bool gravityEnabled;
    public float playerVelocityY = 0.0f;
    [SerializeField]
    private float gravityAmount = -20f;
    [SerializeField]
    private float jumpForce = 10f;

    void Start()
    {
        isRunning = false;
        characterController = GetComponent<CharacterController>();
    }

    void HandleMovement()
    {
        //Getting the inputs for the player
        Vector2 targetDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        //Normalizing the Vector so we get a constant rate of 1. This makes sure we don't exceed the value of 1 when moving diagonally which normally produces a rate of 1.41.
        targetDirection.Normalize();

        //This grabs our target vector, passes it through a smooth damp function which allows you to take a set vector and smoothly transition to our target vector.
        //This requires a current Vector(currentDir) and a ref Vector (currentDirVelocity), both of which must be initiallized at zero to insure that we get a consistant result. This is then multiplied by our moveSmoothSpeed float.
        currentDir = Vector2.SmoothDamp(currentDir, targetDirection, ref currentDirVelocity, moveSmoothSpeed);

        //Player Gravity
        if (characterController.isGrounded && playerVelocityY < 0)
        {
            playerVelocityY = 0.0f;
        }
        //These if statements allow the user to control whether gravity is a force or not.
        if (!gravityEnabled)
        {
            gravityAmount = 0f;
        }

        else
        {
            gravityAmount = -20f;
        }

        //This takes the gravityAmount float and adds it to the playerVelocity vector on the Y axis. This is what insures that gravity has force and affects the players upward movement.
        playerVelocityY += gravityAmount * Time.deltaTime;


        //Jump


        //Player movement begins
        Vector3 walkVelocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * walkSpeed + Vector3.up * playerVelocityY;
        Vector3 runVelocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * runSpeed + Vector3.up * playerVelocityY;

        if (Input.GetKey(KeyCode.LeftShift) && characterController.isGrounded && !isRunning)
        {
            isRunning = true;
            characterController.Move(runVelocity * Time.deltaTime);
        }

        else
        {
            isRunning = false;
            characterController.Move(walkVelocity * Time.deltaTime);
        }
    }

    void Update()
    {
        HandleMovement();
    }
}
