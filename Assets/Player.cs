using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using TreeEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement Details")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    
    private bool canDoubleJump;

    [Header("Colision Infos")]
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
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        UpdateInAirStatus();

        HandleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleColsions();
        HandleAnimations();
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        
        if(Input.GetKeyDown(KeyCode.Space))
            HandleJump();
    }

    private void HandleWallSlide()
    {
        bool canWallSlide = isWallDetected && rb.velocity.y < 0;
        float yModifer = yInput < 0 ? 1 :.05f;

        if(canWallSlide == false)
            return;
        
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * yModifer);
    }

    private void HandleJump()
    {
        if(isGrounded)
            Jump();
        
        else if(canDoubleJump)
        {
            DoubleJump();
        }
    }

    private void HandleMovement()
    {   
        if(isWallDetected)
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

    private void Jump() => rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    
    private void DoubleJump()
    {
        canDoubleJump = false;
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
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
            HandleInAir();
    }

    private void HandleLanded()
    {
        isInAir = false;
        canDoubleJump = true;
    }

    private void HandleInAir()
    {
        isInAir = true;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (facingDirection * wallCheckDistance), transform.position.y));
    }
}
