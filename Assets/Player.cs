using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    [SerializeField] private float moveSpeed;

    private float xInput;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        HandleMovement();
        HandleAnimations();
    }

    private void HandleMovement()
    {
        rb.velocity = new Vector2(xInput*moveSpeed, rb.velocity.y);
    }

    private void HandleAnimations()
    {          
        anim.SetBool("isRunning", rb.velocity.x != 0);
    }
}
