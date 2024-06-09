using System.Collections;
using System.Collections.Generic;
// Remove the following line if you don't need System.Numerics.Vector3
// using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;

public class Trap_Saw : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sr;

    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private float cooldown = 1;
    [SerializeField] private Transform[] wayPoint;
    private Vector3[] waypointPosition;

    public int wayPointIndex = 1;
    public int moveDirection = 1;

    private bool canMove = true;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        UpdateWaypointInfo();
        transform.position = waypointPosition[0];
    }

    private void Update()
    {
        anim.SetBool("active", canMove);

        if (canMove == false)
            return;

        transform.position = Vector2.MoveTowards(transform.position, waypointPosition[wayPointIndex], moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, waypointPosition[wayPointIndex]) < .1f)
        {
            if (wayPointIndex == waypointPosition.Length - 1 || wayPointIndex == 0)
            {
                moveDirection = moveDirection * -1;
                StartCoroutine(StopMovement(cooldown));
            }

            wayPointIndex = wayPointIndex + moveDirection;
        }
    }

    private void UpdateWaypointInfo()
    {
        waypointPosition = new Vector3[wayPoint.Length]; // Initialize the array

        for (int i = 0; i < wayPoint.Length; i++)
        {
            waypointPosition[i] = wayPoint[i].position;
        }
    }

    private IEnumerator StopMovement(float delay)
    {
        canMove = false;

        yield return new WaitForSeconds(delay);

        canMove = true;
        sr.flipX = !sr.flipX;
    }
}