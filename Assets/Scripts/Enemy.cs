using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Windows;

public class Enemy : MonoBehaviour
{
    protected Animator anim;
    protected Rigidbody2D rb;

    [SerializeField] protected GameObject damageTrigger;
    [Space]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float idleDuration = 1.5f;
    protected float idleTimer;

    [Header("Death details")]
    [SerializeField] private float deathImpactSpeed = 5;
    [SerializeField] private float deathRotationSpeed = 150;
    protected bool isDead;
    private int deathRotationDirection = 1;

    [Header("Basic Collision")]
    [SerializeField] protected float groundCheckDistance = 1.1f;
    [SerializeField] protected float wallCheckDistance = .7f;
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] protected Transform groundCheck;
    protected bool isGrounded;
    protected bool isWallDetected;
    protected bool isGroundInFrontDetected;

    protected int facingDirection = -1;
    protected bool isFacingRight = false;


    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        idleTimer -= Time.deltaTime;

        if(isDead)
            HandleDeath();
    }

    public virtual void Die()
    {
        damageTrigger.SetActive(false);
        anim.SetTrigger("hit");
        rb.velocity = new Vector2(rb.velocity.x, deathImpactSpeed);
        isDead = true;

        if (Random.Range(0, 100) < 50)
            deathRotationDirection = deathRotationDirection * -1;
    }

    private void HandleDeath()
    {
        transform.Rotate(0, 0, (deathRotationSpeed * deathRotationDirection) * Time.deltaTime);
    }

    protected virtual void HandleCollisions()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isGroundInFrontDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
    }

    protected virtual void Flip()
    {
        facingDirection *= -1;
        transform.Rotate(0, 180, 0);
        isFacingRight = !isFacingRight;
    }

    protected virtual void HandleFlip(float xValue)
    {
        if (xValue < 0 && isFacingRight || xValue > 0 && !isFacingRight)
            Flip();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (facingDirection * wallCheckDistance), transform.position.y));
    }
}
