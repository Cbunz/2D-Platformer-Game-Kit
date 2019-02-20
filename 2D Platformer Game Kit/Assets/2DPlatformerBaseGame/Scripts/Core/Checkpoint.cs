using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour {

    public bool respawnFacingLeft;

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerCharacter character = collision.GetComponent<PlayerCharacter>();
        if (character != null)
        {
            character.SetCheckpoint(this);
        }
    }
}
