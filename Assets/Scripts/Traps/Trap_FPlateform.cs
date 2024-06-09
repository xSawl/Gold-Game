using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap_FPlateform : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private BoxCollider2D[] colliders;


    [SerializeField] private float speed = .75f;
    [SerializeField] private float travelDistance;
    private Vector3[] waypoints;
    private int waypointsIndex;
    private bool canMove = false;

    [Header("Plateform fall details")]
    [SerializeField] private float impactSpeed = 3;
    [SerializeField] private float impactDuration = .1f;
    private float impactTimer;
    private bool impactHappend;

    [Space]
    [SerializeField] private float fallDelay = .5f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<BoxCollider2D>();
    }
    
    private void Start()
    {
        SetupWaypoint();
        float randomDelay = Random.Range(0, .6f);
        Invoke(nameof(ActivatePlateform), randomDelay);
    }

    private void ActivatePlateform() => canMove = true;

    private void SetupWaypoint()
    {
        waypoints = new Vector3[2];

        float yOffset = travelDistance / 2;

        waypoints[0] = transform.position + new Vector3(0, yOffset, 0);
        waypoints[1] = transform.position + new Vector3(0, -yOffset, 0);
    }

    void Update()
    {
        HandleImpact();
        HandleMovement();
    }

    private void HandleMovement()
    {
        if(canMove == false)
            return;

        transform.position = Vector2.MoveTowards(transform.position, waypoints[waypointsIndex], speed * Time.deltaTime);

        if(Vector2.Distance(transform.position, waypoints[waypointsIndex]) < .1f)
        {
            waypointsIndex ++;

            if(waypointsIndex >= waypoints.Length)
                waypointsIndex = 0;
        }
    }

    private void HandleImpact()
    {
        if (impactTimer < 0)
            return;

        impactTimer -= Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, transform.position + (Vector3.down * 10), impactSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (impactHappend)
            return;

        Player player = other.gameObject.GetComponent<Player>();

        if(player != null)
        {
            Invoke(nameof(SwitchOffPlateform), fallDelay);
            impactTimer = impactDuration;
            impactHappend = true;
        }
    }

    private void SwitchOffPlateform()
    {
        anim.SetTrigger("deactivate");

        canMove = false;
        rb.isKinematic = false;
        rb.gravityScale = 3.5f;
        rb.drag = .5f;

        foreach(BoxCollider2D collider in colliders)
        {
            collider.enabled = false;
        }


    }

}
