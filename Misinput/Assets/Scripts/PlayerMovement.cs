using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovementTutorial : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    // === Ability Locks ===
    private bool canMoveForward = true;
    private bool canMoveBackward = false;
    private bool canMoveLeft = false;
    private bool canMoveRight = false;
    private bool canJump = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();

        // handle drag
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        float rawH = Input.GetAxisRaw("Horizontal");
        float rawV = Input.GetAxisRaw("Vertical");

        // restrict input based on unlocked keys
        horizontalInput = 0f;
        verticalInput = 0f;

        if (canMoveForward && rawV > 0)
            verticalInput = rawV;
        if (canMoveBackward && rawV < 0)
            verticalInput = rawV;
        if (canMoveRight && rawH > 0)
            horizontalInput = rawH;
        if (canMoveLeft && rawH < 0)
            horizontalInput = rawH;

        // handle jump input
        if (canJump && Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    // === Called by KeyPickup ===
    public void UnlockAbility(string key)
    {
        switch (key)
        {
            case "W":
                canMoveForward = true;
                break;
            case "A":
                canMoveLeft = true;
                break;
            case "S":
                canMoveBackward = true;
                break;
            case "D":
                canMoveRight = true;
                break;
            case "Space":
                canJump = true;
                break;
            default:
                Debug.LogWarning("Unknown ability key: " + key);
                break;
        }

        Debug.Log("Unlocked: " + key);
    }
}
