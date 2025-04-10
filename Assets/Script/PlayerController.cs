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
    private float moveHoldTime = 0f;
    private float maxSpeed = 10f;
    private float speedRate = 5f;
    [SerializeField] private float maxJumpForce = 12.5f;
    [SerializeField] private float slopeFactor = 0.5f;
    public bool isOnLadder = false;
    public float climbSpeed = 4f;
    private float originalGravityScale;

    // Tambahan untuk launching dari tangga
    private bool isLaunchingFromLadder = false;
    private float launchDuration = 0.2f;
    private float launchTimer = 0f;

    public bool isHanging = false;
    public Transform hangingCheck;
    public LayerMask hangingLayer;
    public float hangingCheckRadius = 0.2f;
    public float hangingMoveSpeed = 3f;


    public float CurrentMoveSpeed
    {
        get
        {
            if (IsMoving && touchingDirection.IsGrounded)
            {
                float calculatedSpeed = speedRate * moveHoldTime;
                return Mathf.Min(calculatedSpeed, maxSpeed);
            }
            else if (IsMoving && !touchingDirection.IsGrounded)
            {
                float calculatedSpeed = speedRate * moveHoldTime;
                return Mathf.Min(calculatedSpeed, maxSpeed);
            }
            else
            {
                return 0f;
            }
        }
    }

    public float jumpImpulse = 10f;

    Vector2 moveInput;
    TouchingDirection touchingDirection;

    [SerializeField]
    private bool _isMoving = false;

    [SerializeField]
    private bool _isRunning = false;

    public bool IsRunning
    {
        get { return _isRunning; }
        set
        {
            _isRunning = value;
            animator.SetBool(AnimationStrings.isRunning, value);
        }
    }

    public bool IsMoving
    {
        get { return _isMoving; }
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
        originalGravityScale = rb.gravityScale;
    }

    void Start() { }

    void Update() { }

    private void FixedUpdate()
    {
        if (IsMoving && touchingDirection.IsGrounded)
        {
            moveHoldTime += Time.fixedDeltaTime;
        }
        else if (!touchingDirection.IsGrounded && moveInput.x != 0)
        {
            // hold time tetap
        }
        else
        {
            moveHoldTime = 0f;
        }

        if (isOnLadder)
        {
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(rb.velocity.x, moveInput.y * climbSpeed);
            animator.SetBool(AnimationStrings.isClimbing, moveInput.y != 0);
        }
        else
        {
            rb.gravityScale = originalGravityScale;
            animator.SetBool(AnimationStrings.isClimbing, false);
        }

        if (!isLaunchingFromLadder)
        {
            rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);
        }
        else
        {
            launchTimer -= Time.fixedDeltaTime;
            if (launchTimer <= 0f)
            {
                isLaunchingFromLadder = false;
            }
        }
        if (!touchingDirection.IsGrounded && !isOnLadder && !isHanging)
        {
            Collider2D hangingSpot = Physics2D.OverlapCircle(hangingCheck.position, hangingCheckRadius, hangingLayer);

            if (hangingSpot != null && rb.velocity.y > 0) // Naik dan kena bawah platform
            {
                EnterHangingMode();
            }
        }
        if (isHanging)
        {
            rb.velocity = new Vector2(moveInput.x * hangingMoveSpeed, 0f);

            if (moveInput != Vector2.zero)
            {
                setFacingDirection(moveInput);
            }
        }

        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        moveInput = input;

        IsMoving = moveInput != Vector2.zero;
        setFacingDirection(moveInput);

        if (moveInput.x == 0)
        {
            moveHoldTime = 0f;
        }
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
            isFacingRight = true;
        }
        else if (moveInput.x < 0 && isFacingRight)
        {
            isFacingRight = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isHanging)
            {
                ExitHangingMode();
                return;
            }

            if (touchingDirection.IsGrounded)
            {
                float horizontalSpeed = Mathf.Abs(rb.velocity.x);
                float adjustedJumpForce = CalculateJumpForceFromSpeed(horizontalSpeed);
                float horizontalBoost = CalculateHorizontalBoost(horizontalSpeed);
                float direction = Mathf.Sign(moveInput.x);
                rb.velocity = new Vector2(rb.velocity.x + horizontalBoost * direction, adjustedJumpForce);
                animator.SetTrigger(AnimationStrings.jump);
            }
        }
    }


    private float CalculateJumpForceFromSpeed(float horizontalSpeed)
    {
        float minSpeed = 0f;
        float maxSpeed = 8f;
        float maxJump = 11f;
        float minJump = 8f;

        float t = horizontalSpeed / maxSpeed;
        float jumpForce = Mathf.Lerp(maxJump, minJump, Mathf.Sqrt(t));
        return jumpForce;
    }

    private float CalculateHorizontalBoost(float horizontalSpeed)
    {
        float maxBoost = 3f;
        float minBoost = 0.5f;
        float t = Mathf.Clamp01(horizontalSpeed / 10f);
        return Mathf.Lerp(maxBoost, minBoost, t);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("UjungTangga"))
        {
            isOnLadder = false;
            rb.gravityScale = originalGravityScale;
            animator.SetBool(AnimationStrings.isClimbing, false);

            float launchForceX = 6f;
            float launchForceY = 8f;
            float direction = isFacingRight ? 1f : -1f;

            rb.velocity = new Vector2(launchForceX * direction, launchForceY);
            animator.SetTrigger(AnimationStrings.jump);

            isLaunchingFromLadder = true;
            launchTimer = launchDuration;
        }
    }
    void EnterHangingMode()
    {
        isHanging = true;
        rb.velocity = Vector2.zero;

        rb.gravityScale = 0f;

        //  Kunci posisi vertikal biar gak turun-turun
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        animator.SetBool("isHanging", true); // Buat animasi kalau ada
    }

    void ExitHangingMode()
    {
        isHanging = false;

        rb.gravityScale = originalGravityScale;

        //  Buka kunci posisi Y biar bisa jatuh lagi
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        animator.SetBool("isHanging", false);
    }


}
