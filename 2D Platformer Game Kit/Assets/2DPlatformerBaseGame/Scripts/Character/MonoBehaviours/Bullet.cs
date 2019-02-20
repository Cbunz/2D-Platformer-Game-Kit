using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Damager))]
public class Bullet : MonoBehaviour {

    public bool destroyWhenOutOfView = true;
    public bool spriteOriginallyFacesLeft;

    public float timeBeforeAutodestruct = -1.0f;

    [HideInInspector]
    public BulletObject bulletPoolObject;
    [HideInInspector]
    public Camera mainCamera;

    protected SpriteRenderer spriteRenderer;

    const float offScreenError = 0.01f;

    protected float timer;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = 0.0f;
    }

    public void ReturnToPool()
    {
        bulletPoolObject.ReturnToPool();
    }

    private void FixedUpdate()
    {
        if (destroyWhenOutOfView)
        {
            Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
            bool onScreen = screenPoint.z > 0 && screenPoint.x > -offScreenError &&
                            screenPoint.x < (1 + offScreenError) && screenPoint.y > -offScreenError &&
                            screenPoint.y < (1 + offScreenError);
            if (!onScreen)
                bulletPoolObject.ReturnToPool();
        }

        if (timeBeforeAutodestruct > 0)
        {
            timer += Time.deltaTime;
            if (timer > timeBeforeAutodestruct)
                bulletPoolObject.ReturnToPool();
        }
    }

    public void OnHitDamageable(Damager origin, Damageable damageable)
    {
        FindSurface(origin.LastHit);
        bulletPoolObject.ReturnToPool();
    }

    public void OnHitNonDamageable(Damager origin)
    {
        FindSurface(origin.LastHit);
        bulletPoolObject.ReturnToPool();
    }

    protected void FindSurface(Collider2D collider)
    {
        Vector3 forward = spriteOriginallyFacesLeft ? Vector3.left : Vector3.right;
        if (spriteRenderer.flipX)
        {
            forward.x = -forward.x;
        }

        // TileBase surfaceHit = PhysicsHelper.FindTileForOverride(collider, transform.position, forward);

        // VFXController.Instance.Trigger(VFX_HASH, transform.position, 0, spriteRenderer.flipX, null, surfaceHit);
    }
}
