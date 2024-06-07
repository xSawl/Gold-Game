using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
   private Animator anim => GetComponent<Animator>();
   private bool isActive;

   private void OnTriggerEnter2D(Collider2D other)
   {
        if(isActive)
            return;
        
        Player player = other.GetComponent<Player>();

        if(player != null)
            ActivateCheckPoint();
   }

   private void ActivateCheckPoint()
   {
        isActive = true;
        anim.SetTrigger("activate");
        GameManager.instance.UpdateRespawnPosition(transform);
   }

}
