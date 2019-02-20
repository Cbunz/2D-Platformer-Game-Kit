using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownShield : MonoBehaviour
{
    private new Rigidbody2D rigidbody;
    
    [HideInInspector]
    public Vector2 direction;

    public float rotateAmount = 50;
    public float speed = 5f;
    public float returnSpeed = 5f;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rigidbody.velocity = new Vector2(PlayerCharacter.Instance.shield.shieldX, PlayerCharacter.Instance.shield.shieldY).normalized * speed;
        Debug.Log(rigidbody.velocity);
    }

    private void FixedUpdate()
    {
        if (PlayerCharacter.Instance.shield.shieldReturn)
        {
            Vector2 returnDirection = (PlayerCharacter.Instance.transform.position - this.transform.position).normalized;
            rigidbody.velocity = returnDirection * returnSpeed;
        }
        else
            rigidbody.velocity = rigidbody.velocity.normalized * speed;

        if (rigidbody.velocity.x < 0)
            transform.Rotate(0, 0, rotateAmount * Time.deltaTime);
        else
            transform.Rotate(0, 0, -rotateAmount * Time.deltaTime);
    }
}
