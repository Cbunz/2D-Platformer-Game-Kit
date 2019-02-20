using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private Damageable damageable;
    private float tanLaunchAngle;
    private bool launch = false;
    private bool launched = false;

    public bool Launched { get { return launched; } }

    public bool canLaunch = true;
    [Range(minLaunchAngle, maxLaunchAngle)] public float launchAngle = 45f;
    public float launchAngleVariance = 15f;
    public float launchSpeed = 10f;
    public float launchSpeedVariance = 3f;
    public LayerMask hittable;

    protected const float minLaunchAngle = 0.001f;
    protected const float maxLaunchAngle = 89.999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        damageable = GetComponent<Damageable>();
    }

    private void Start()
    {
        launchAngle = Mathf.Clamp(launchAngle, minLaunchAngle, maxLaunchAngle);
        tanLaunchAngle = Mathf.Tan(Mathf.Deg2Rad * launchAngle);
    }

    private void FixedUpdate()
    {
        if (launch)
        {
            Launch();

            if (animator != null)
                animator.enabled = false;
        }
    }

    public void ActivateLaunch()
    {
        launch = true;
    }

    void Launch()
    {
        SetRandomLaunchAngle();
        Vector2 direction = GetLaunchDirection();
        float speed = GetRandomLaunchSpeed();
        rb.velocity = direction * speed;
        launch = false;
        launched = true;
    }

    Vector2 GetLaunchDirection()
    {
        Vector2 damageDirection = damageable.GetDamageDirection();

        // if (damageDirection.y < 0f)
        // return new Vector2(Mathf.Sign(damageDirection.x), 0f);

        float y = Mathf.Abs(damageDirection.x) * tanLaunchAngle;

        return new Vector2(damageDirection.x, y).normalized;
    }

    void SetRandomLaunchAngle()
    {
        float angleVariation = Random.Range(-launchAngleVariance, launchAngleVariance);
        tanLaunchAngle = Mathf.Tan(Mathf.Deg2Rad * (launchAngle + angleVariation));
    }

    float GetRandomLaunchSpeed()
    {
        float speedVariation = Random.Range(-launchSpeedVariance, launchSpeedVariance);
        return launchSpeed + speedVariation;
    }
}
