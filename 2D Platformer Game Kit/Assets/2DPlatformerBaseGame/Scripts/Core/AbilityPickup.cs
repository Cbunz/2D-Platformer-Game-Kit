using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPickup : MonoBehaviour {
    
	public enum AbilityType
    {
        sword,
        shield,
        gun
    }

    public AbilityType type;
    public int durability;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 9)
        {
            PlayerCharacter player = collision.gameObject.GetComponent<PlayerCharacter>();

            switch (type)
            {
                case AbilityType.sword:
                    player.swordDurability = durability;
                    player.EnableMeleeAttack();
                    break;
                case AbilityType.shield:
                    player.shield.EnableShield();
                    break;
                case AbilityType.gun:
                    player.gunAmmo = durability;
                    player.EnableRangedAttack();
                    break;
                default:
                    Debug.Log("Incorrect ability type.");
                    break;
            }

            Destroy(gameObject);
        }
    }
}
