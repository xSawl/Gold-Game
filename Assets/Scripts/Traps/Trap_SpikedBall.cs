using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap_SpikedBall : MonoBehaviour
{
    [SerializeField] private Rigidbody2D spikeRB;
    [SerializeField] private float pushForce;
    [SerializeField] private float maintainSpeedThreshold = 1f; // Seuil pour la vitesse minimale � maintenir

    private void Start()
    {
        float randomDelay = Random.Range(0, .6f);
        Invoke(nameof(ActivateSpikeMovement), randomDelay);
    }

    private void FixedUpdate()
    {
        // V�rifiez si la vitesse actuelle est inf�rieure au seuil pour r�appliquer la force
        if (Mathf.Abs(spikeRB.velocity.x) < maintainSpeedThreshold)
        {
            Vector2 maintainForce = new Vector2(pushForce * Mathf.Sign(spikeRB.velocity.x), 0);
            spikeRB.AddForce(maintainForce, ForceMode2D.Force);
        }
    }

    private void ActivateSpikeMovement()
    {
        Vector2 pushVector = new Vector2(pushForce, 0);
        spikeRB.AddForce(pushVector, ForceMode2D.Impulse);
    }
}
