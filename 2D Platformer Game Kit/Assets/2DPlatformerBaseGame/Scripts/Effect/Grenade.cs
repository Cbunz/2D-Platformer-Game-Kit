using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public Vector2 initialForce;
    public float timer = 1;
    public GameObject explosion;
    public float explosionTimer = 3;

    new Rigidbody2D rigidbody;

    private void OnEnable()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    IEnumerator Start()
    {
        rigidbody.AddForce(initialForce);
        yield return new WaitForSeconds(timer);
        var eGo = Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(eGo, explosionTimer);
        Destroy(gameObject);
    }
}
