using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthPickup : MonoBehaviour {

    public int healthAmount = 1;
    public UnityEvent OnGivingHealth;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == PlayerCharacter.Instance.gameObject)
        {
            Damageable damageable = PlayerCharacter.Instance.damageable;
            if (damageable.CurrentHealth < damageable.startingHealth)
            {
                damageable.GainHealth(Mathf.Min(healthAmount, damageable.startingHealth - damageable.CurrentHealth));
                OnGivingHealth.Invoke();
            }
        }
    }
}
