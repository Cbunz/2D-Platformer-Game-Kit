using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderTrap : MonoBehaviour
{
    private Transform boulder;
    private Rigidbody2D boulderRB;
    private Damager damager;
    new private BoxCollider2D collider;

	private void Awake ()
    {
        boulder = transform.Find("Boulder");
        boulderRB = boulder.GetComponent<Rigidbody2D>();
        collider = boulder.GetComponent<BoxCollider2D>();
        damager = boulder.GetComponent<Damager>();
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 9)
            boulderRB.bodyType = RigidbodyType2D.Dynamic;
    }

    public void DisableBoulder()
    {
        collider.enabled = false;
        damager.enabled = false;
    }
}
