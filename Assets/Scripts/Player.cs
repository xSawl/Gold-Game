using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Numerics;
using TreeEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

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

    [Header("Wall Interractions")]
    [SerializeField] private float wallJumpDuration = .6f;
    [SerializeField] private Vector2 wallJumpForce;
    private bool isWallJumping;

    [Header("Knockback")]
    [SerializeField] private float KnockbackDuration =1;
    [SerializeField] private Vector2 knockBackPower;
    private bool isKnocked;

    [Header("Jump")]
    [SerializeField] private float bufferJumpWindow = .25f;
    private float bufferJumpActivated = -1;
    [SerializeField] private float coyoteJumpWindow = .5f;
    private float coyoteJumpActivated = -1;
    
    [Header("VFX")]
    [SerializeField] private GameObject deathVFX;


    [Header("Colision")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;
    private bool isInAir;
    private bool isWallDetected;

    private float xInput;
    private float yInput;

    private bool isFacingRight = true;
    private int facingDirection = 1;

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

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
            Knockback();

        UpdateInAirStatus();

        if(canBeControlled == false)
            return;

        if(isKnocked)
            return;

        HandleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleColsions();
        HandleAnimations();
    }

    public void RespawnFinished(bool finished)
    {
        if(finished) 
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

    public void Knockback()
    {
        if(isKnocked)
            return;
            
        StartCoroutine(knockbackRoutine());
        anim.SetTrigger("knockback");

        rb.velocity = new Vector2(knockBackPower.x * -facingDirection, knockBackPower.y);
    }

    private IEnumerator knockbackRoutine()
    {
        isKnocked = true;
        yield return new WaitForSeconds(KnockbackDuration);
        isKnocked = false;
    }

    public void Die()
    {
        GameObject newDeathVFX = Instantiate(deathVFX, transform.position, UnityEngine.Quaternion.identity);
        Destroy(gameObject);
    }


    private void HandleMovement()
    {   
        if(isWallDetected)
            return;
        
        if(isWallJumping)
            return;

        rb.velocity = new Vector2(xInput*moveSpeed, rb.velocity.y);
    }

    private void HandleFlip()
    {
        if(xInput < 0 && isFacingRight || xInput > 0 && !isFacingRight)
            Flip();
    }

    private void HandleAnimations()
    {          
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallDetected", isWallDetected);
    }

    private void HandleColsions()
    {          
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        
        if(Input.GetKeyDown(KeyCode.Space))
        {
            HandleJump();
            RequestBufferJump();
        }
            
    }
    
    #region Buffer & Coyote Jump
    private void RequestBufferJump()
    {
        if(isInAir)
            bufferJumpActivated = Time.time;
    }

    private void AttemptBufferJump()
    {
        if(Time.time < bufferJumpActivated + bufferJumpWindow) 
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
        bool coyoteJumpAvailable =  Time.time < coyoteJumpActivated  + coyoteJumpWindow;

        if(isGrounded || coyoteJumpAvailable) 
        {
            Jump();
        }
            

        else if(isWallDetected && !isGrounded)
        {
            WallJump();
        }
            
        else if(canDoubleJump)
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
        float yModifer = yInput < 0 ? 1 :.05f;

        if(canWallSlide == false)
            return;
        
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * yModifer);
    }

    private void Flip() 
    {
        facingDirection = facingDirection * -1;
        transform.Rotate(0, 180, 0);
        isFacingRight = !isFacingRight;
    }

    private void UpdateInAirStatus()
    {
        if(isGrounded  && isInAir )
            HandleLanded();

        if(!isGrounded && !isInAir )
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

        if(rb.velocity.y <= 0)
        {
            ActivateCoyoteJump();
        } 
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (facingDirection * wallCheckDistance), transform.position.y));
    }
}
