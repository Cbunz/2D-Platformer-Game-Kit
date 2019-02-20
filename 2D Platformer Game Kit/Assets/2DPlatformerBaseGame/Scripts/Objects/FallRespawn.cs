using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Respawns the character when they fall off the level.
/// </summary>

public class FallRespawn : MonoBehaviour
{
    public int damage = 1;
    public float damageDelay = 0.5f; // Time delay for damage processing to avoid losing health on UI before fade out.
    
    private Damageable playerDamageable;

    private void Awake()
    {
        playerDamageable = PlayerCharacter.Instance.GetComponent<Damageable>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 9) // If the object is the player...
        {
            PlayerCharacter.Instance.TriggerDieRespawn(false, true, 0); // Trigger player's respawn function (false, true, 0) is (resetHealth = false, useCheckpoint = true, waitTime = 0)
            Invoke("TakeFallDamage", damageDelay); // Take damage
        }
    }

    private void TakeFallDamage()
    {
        PlayerCharacter.Instance.damageable.SetHealth(playerDamageable.CurrentHealth - damage); // Subtract damage amount from health.
    }
}
