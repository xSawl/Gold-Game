using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D cd;

    private bool canBeControlled = false;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private float defaultGravityScale;
    private bool canDoubleJump;

    [Header("Acceleration")]
    [SerializeField] private float runAccelAmount = 10f;
    [SerializeField] private float runDeccelAmount = 20f;
    [SerializeField] private float accelInAir = 0.5f;
    [SerializeField] private float deccelInAir = 0.5f;
    [SerializeField] private bool doConserveMomentum = true;

    [Header("Bonus Jump Apex")]
    [SerializeField] private float jumpHangTimeThreshold = 0.1f;
    [SerializeField] private float jumpHangAccelerationMult = 1.2f;
    [SerializeField] private float jumpHangMaxSpeedMult = 1.1f;

    [Header("Wall Interactions")]
    [SerializeField] private float wallJumpDuration = 0.6f;
    [SerializeField] private Vector2 wallJumpForce;
    private bool isWallJumping;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 1f;
    [SerializeField] private Vector2 knockbackPower;
    private bool isKnocked;

    [Header("Jump")]
    [SerializeField] private float bufferJumpWindow = 0.25f;
    private float bufferJumpActivated = -1f;
    [SerializeField] private float coyoteJumpWindow = 0.5f;
    private float coyoteJumpActivated = -1f;

    [Header("VFX")]
    [SerializeField] private GameObject deathVFX;

    [Header("Collision")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [Space]
    [SerializeField] private Transform enemyCheck;
    [SerializeField] private float enemyCheckRadius;
    [SerializeField] private LayerMask whatisEnemy;
    private bool isGrounded;
    private bool isInAir;
    private bool isWallDetected;

    private float xInput;
    private float yInput;

    private bool isFacingRight = true;
    private int facingDirection = 1;

    private bool IsJumping => !isGrounded && rb.velocity.y > 0;
    private bool IsWallJumping => isWallJumping;
    private bool IsJumpFalling => !isGrounded && rb.velocity.y < 0;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CapsuleCollider2D>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        defaultGravityScale = rb.gravityScale;
        RespawnFinished(false);
    }

    private void Update()
    {
        UpdateInAirStatus();

        if (!canBeControlled || isKnocked)
            return;

        HandleEnemyDetection();
        HandleInput();
        HandleFlip();
        HandleAnimations();
    }


    private void FixedUpdate()
    {
        if (!canBeControlled || isKnocked)
            return;

        HandleMovement();
        HandleWallSlide();
        HandleCollisions();
    }

    private void HandleEnemyDetection()
    {
        if (rb.velocity.y >= 0)
            return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(enemyCheck.position, enemyCheckRadius, whatisEnemy);

        foreach (var enemy in colliders)
        {
            Enemy newEnemy = enemy.GetComponent<Enemy>();
            if (newEnemy != null)
            {
                newEnemy.Die();
                Jump();
            }
        }
    }

    public void RespawnFinished(bool finished)
    {
        if (finished)
        {
            rb.gravityScale = defaultGravityScale;
            canBeControlled = true;
            cd.enabled = true;
        }
        else
        {
            rb.gravityScale = 0;
            canBeControlled = false;
            cd.enabled = false;
        }
    }

    public void Knockback(float sourceDamageXPosition)
    {
        float knockbackDirection = 1;

        if(transform.position.x < sourceDamageXPosition)
            knockbackDirection = -1;
        if (isKnocked)
            return;
        
        StartCoroutine(KnockbackRoutine());
        rb.velocity = new Vector2(knockbackPower.x * knockbackDirection, knockbackPower.y);
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnocked = true;
        anim.SetBool("isKnocked", true);

        yield return new WaitForSeconds(knockbackDuration);
        
        isKnocked = false;
        anim.SetBool("isKnocked", false);
    }

    public void Die()
    {
        Instantiate(deathVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void HandleMovement()
    {
        if (isWallDetected || isWallJumping)
            return;

        float targetSpeed = xInput * moveSpeed;

        float accelRate;
        if (isGrounded)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount : runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount * accelInAir : runDeccelAmount * deccelInAir;

        if (doConserveMomentum && Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && !isGrounded)
        {
            accelRate = 0;
        }

        if ((IsJumping || IsWallJumping || IsJumpFalling) && Mathf.Abs(rb.velocity.y) < jumpHangTimeThreshold)
        {
            accelRate *= jumpHangAccelerationMult;
            targetSpeed *= jumpHangMaxSpeedMult;
        }

        float speedDif = targetSpeed - rb.velocity.x;
        float movement = speedDif * accelRate;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void HandleFlip()
    {
        if (xInput < 0 && isFacingRight || xInput > 0 && !isFacingRight)
            Flip();
    }

    private void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallDetected", isWallDetected);
    }

    private void HandleCollisions()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleJump();
            RequestBufferJump();
        }
    }

    #region Buffer & Coyote Jump
    private void RequestBufferJump()
    {
        if (isInAir)
            bufferJumpActivated = Time.time;
    }

    private void AttemptBufferJump()
    {
        if (Time.time < bufferJumpActivated + bufferJumpWindow)
        {
            bufferJumpActivated = Time.time - 1;
            Jump();
        }
    }

    private void ActivateCoyoteJump() => coyoteJumpActivated = Time.time;
    private void CancelCoyoteJump() => coyoteJumpActivated = Time.time - 1;
    #endregion

    private void HandleJump()
    {
        bool coyoteJumpAvailable = Time.time < coyoteJumpActivated + coyoteJumpWindow;

        if (isGrounded || coyoteJumpAvailable)
        {
            Jump();
        }
        else if (isWallDetected && !isGrounded)
        {
            WallJump();
        }
        else if (canDoubleJump)
        {
            DoubleJump();
        }

        CancelCoyoteJump();
    }

    private void Jump() => rb.velocity = new Vector2(rb.velocity.x, jumpForce);

    private void DoubleJump()
    {
        isWallJumping = false;
        canDoubleJump = false;
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
    }

    private void WallJump()
    {
        rb.velocity = new Vector2(wallJumpForce.x * -facingDirection, wallJumpForce.y);
        Flip();

        StopAllCoroutines();
        StartCoroutine(WallJumpRoutine());
    }

    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }

    private void HandleWallSlide()
    {
        bool canWallSlide = isWallDetected && rb.velocity.y < 0;
        float yModifier = yInput < 0 ? 1f : 0.05f;

        if (!canWallSlide)
            return;

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * yModifier);
    }

    private void Flip()
    {
        facingDirection *= -1;
        transform.Rotate(0, 180, 0);
        isFacingRight = !isFacingRight;
    }

    private void UpdateInAirStatus()
    {
        if (isGrounded && isInAir)
            HandleLanded();

        if (!isGrounded && !isInAir)
        {
            BecomeAirborne();
        }
    }

    private void HandleLanded()
    {
        isInAir = false;
        canDoubleJump = true;
        AttemptBufferJump();
    }

    private void BecomeAirborne()
    {
        isInAir = true;

        if (rb.velocity.y <= 0)
        {
            ActivateCoyoteJump();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(enemyCheck.position, enemyCheckRadius);
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (facingDirection * wallCheckDistance), transform.position.y));
    }
}
