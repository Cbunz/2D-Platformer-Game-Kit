using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Pushable : MonoBehaviour
{
    static ContactPoint2D[] contactPointBuffer = new ContactPoint2D[16];
    static Dictionary<Collider2D, Pushable> pushableCache = new Dictionary<Collider2D, Pushable>();

    public Transform playerPushingRightPosition;
    public Transform playerPushingLeftPosition;
    public Transform pushablePosition;

    public AudioSource pushableAudioSource;
    public AudioClip startingPushClip;
    public AudioClip loopPushClip;
    public AudioClip endPushClip;

    public bool Grounded { get { return grounded; } }

    protected SpriteRenderer spriteRenderer;
    new protected Rigidbody2D rigidbody;
    protected bool grounded;
    Collider2D[] waterColliders;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody = GetComponent<Rigidbody2D>();

        if (pushableCache.Count == 0)
        {
            Pushable[] pushables = FindObjectsOfType<Pushable>();

            for (int i = 0; i < pushables.Length; i++)
            {
                Collider2D[] pushableColliders = pushables[i].GetComponents<Collider2D>();

                for (int j = 0; j < pushableColliders.Length; j++)
                {
                    pushableCache.Add(pushableColliders[j], pushables[i]);
                }
            }
        }
        /*
        WaterArea[] waterAreas = FindObjectsOfType<WaterArea>();
        waterColliders = new Collider2D[waterAreas.Length];
        for (int i = 0; i < waterAreas.Length; i++)
        {
            waterColliders[i] = waterAreas[i].GetComponent<Collider2D>();
        }
        */
    }

    private void FixedUpdate()
    {
        Vector2 velocity = rigidbody.velocity;
        velocity.x = 0f;
        rigidbody.velocity = velocity;

        CheckGrounded();

        /*
        for (int i = 0; i < waterColliders.Length; i++)
        {
            if (rigidbody2D.IsTouching(waterColliders[i]))
            {
                rigidbody2D.constraints |= RigidbodyConstraints2D.FreezePositionX;
            }
        }
        */
    }

    public void StartPushing()
    {
        pushableAudioSource.loop = false;
        pushableAudioSource.clip = startingPushClip;
        pushableAudioSource.Play();
    }

    public void EndPushing()
    {
        pushableAudioSource.loop = false;
        pushableAudioSource.clip = endPushClip;
        pushableAudioSource.Play();
    }

    public void Move(Vector2 movement)
    {
        //Vector2 position = rigidbody.position;
        //rigidbody.MovePosition(position + movement);
        rigidbody.position = rigidbody.position + movement;

        if (!pushableAudioSource.isPlaying)
        {
            pushableAudioSource.clip = loopPushClip;
            pushableAudioSource.loop = true;
            pushableAudioSource.Play();
        }
    }

    protected void CheckGrounded()
    {
        grounded = false;

        int count = rigidbody.GetContacts(contactPointBuffer);
        for (int i = 0; i < count; ++i)
        {
            if (contactPointBuffer[i].normal.y > 0.9f)
            {
                grounded = true;

                Pushable pushable;

                if (pushableCache.TryGetValue(contactPointBuffer[i].collider, out pushable))
                {
                    spriteRenderer.sortingOrder = pushable.spriteRenderer.sortingOrder + 1;
                }
            }
        }
    }
}
