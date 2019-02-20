using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour {

    private Shatter shatter;
    private bool canShatter = true;
    private BoxCollider2D coll;
    private Damager damager;

    private void Awake()
    {
        shatter = GetComponent<Shatter>();
        coll = GetComponent<BoxCollider2D>();
        damager = GetComponent<Damager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (canShatter)
        {
            shatter.shatter();
            damager.enabled = false;
            coll.enabled = false;
            canShatter = false;
        }
    }
}
