using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishPoint : MonoBehaviour
{

    private Animator anim => GetComponent<Animator>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();

        if(player != null)
        {
            anim.SetTrigger("activate");
            Debug.Log("You Finished the level !");
        }
    }
}
