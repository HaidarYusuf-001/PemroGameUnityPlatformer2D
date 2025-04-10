using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirection))]

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float airWalkSpeed = 3f;

    public float CurrentMoveSpeed
    {
        get
        {
            if (IsMoving && !touchingDirection.IsOnWall)
            {
                if (touchingDirection.IsGrounded)
                {
                    if (IsRunning)
                    {
                        return runSpeed;
                    }
                    else
                    {
                        return walkSpeed;
                    }
                }
                else { return airWalkSpeed; }

            }
            else
            {
                return 0;
            }
        }
    }

    public float jumpImpulse = 10f;
    public float jumpDistanceX = 5f; // Jarak horizontal loncatan
    public float jumpDistanceY = 10f; // Jarak vertikal loncatan

    Vector2 moveInput;
    TouchingDirection touchingDirection;

    [SerializeField]
    private bool _isMoving = false;

    [SerializeField]
    private bool _isRunning = false;

    public bool IsRunning { get { return _isRunning; } set { _isRunning = value; animator.SetBool(AnimationStrings.isRunning, value); } }

    public bool IsMoving
    {
        get
        {
            return _isMoving;
        }
        private set
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, value);
        }
    }

    public bool _isFacingRight = true;

    public bool isFacingRight
    {
        get { return _isFacingRight; }
        private set
        {
            if (_isFacingRight != value)
            {
                // Flip the local scale to make the player face the opposite direction
                transform.localScale *= new Vector2(-1, 1);
            }
            _isFacingRight = value;
        }
    }

    Rigidbody2D rb;
    Animator animator;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirection = GetComponent<TouchingDirection>();
        
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame

    void Update()
    {

    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);

        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        // Hanya izinkan gerakan horizontal jika pemain berada di tanah
        if (touchingDirection.IsGrounded)
        {
            moveInput.x = input.x;
        }
        else
        {
            moveInput.x = 0; // Tidak bisa bergerak ke kiri/kanan di udara
        }

        IsMoving = moveInput != Vector2.zero;
        setFacingDirection(moveInput);
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsRunning = true;
        }
        else if (context.canceled)
        {
            IsRunning = false;
        }
    }

    void setFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !isFacingRight)
        {
            // Face right
            isFacingRight = true;
        }
        else if (moveInput.x < 0 && isFacingRight)
        {
            // Face left
            isFacingRight = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && touchingDirection.IsGrounded)
        {
            animator.SetTrigger(AnimationStrings.jump);
            float jumpVelocityX = isFacingRight ? jumpDistanceX : -jumpDistanceX;
            Vector2 jumpForce = new Vector2(jumpVelocityX, jumpDistanceY);
            rb.AddForce(jumpForce, ForceMode2D.Impulse);
        }
    }
}
