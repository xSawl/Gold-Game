using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;

    [Header("Colision Infos")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;

    private float xInput;

    private bool isFacingRight = true;
    private int facingDirection = 1;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        HandleColsions();
        HandleInput();
        HandleMovement();
        HandleFlip();
        HandleAnimations();
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
            Jump();
    }

    private void HandleMovement()
    {
        rb.velocity = new Vector2(xInput*moveSpeed, rb.velocity.y);
    }

    private void HandleFlip()
    {
        if(rb.velocity.x < 0 && isFacingRight || rb.velocity.x > 0 && !isFacingRight)
            Flip();
    }

    private void HandleAnimations()
    {          
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isGrounded", isGrounded);
    }

    private void HandleColsions()
    {          
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    //Jump Function
    private void Jump() => rb.velocity = new Vector2(rb.velocity.x, jumpForce);


    private void Flip() 
    {
        facingDirection = facingDirection * -1;
        transform.Rotate(0, 180, 0);
        isFacingRight = !isFacingRight;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
    }
}
