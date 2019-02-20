using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerProjectile : MonoBehaviour
{
    public Vector2 initialForce;
    public float timer = 1;
    public float fuse = 0.01f;
    public GameObject explosion;
    public float explosionTimer = 3;
    new Rigidbody2D rigidbody;

    protected GameObject hitEffect;

    private void OnEnable()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        Destroy(gameObject, timer);

        hitEffect = Instantiate(explosion);
        hitEffect.SetActive(false);
    }

    public void Destroy()
    {
        hitEffect.transform.position = transform.position;
        hitEffect.SetActive(true);
        GameObject.Destroy(hitEffect, explosionTimer);
        Destroy(gameObject);
    }

    private void Start()
    {
        rigidbody.AddForce(initialForce);
    }
}
