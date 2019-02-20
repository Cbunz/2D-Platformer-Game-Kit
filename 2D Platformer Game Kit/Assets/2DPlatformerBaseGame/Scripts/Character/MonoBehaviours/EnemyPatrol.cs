using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    protected Vector2 moveVector;
    public Vector2 MoveVector { get { return moveVector; } }
    
    public float speed = 10;
    public bool useGravity = false;
    public float gravity = 10f;
    public bool edgeStop = true;
    public bool usePatrolBorders = true;
    public Vector3 patrolLeft = Vector3.zero;
    public Vector3 patrolRight = Vector3.zero;
    public Damager contactDamager;
    public float damagedKnockback;
    public Vector2 knockback;
    public float flickeringDuration;

    public RandomAudioPlayer hurtAudio;
    public RandomAudioPlayer dieAudio;
    public RandomAudioPlayer movementAudio;

    protected SpriteRenderer spriteRenderer;
    protected CharacterController2D characterController2D;
    protected new Rigidbody2D rigidbody2D;
    protected Animator animator;

    protected Coroutine flickeringCoroutine = null;
    protected Color originalColor;

    protected bool spriteOriginallyFacesLeft;
    protected int direction = 1;
    protected bool canTurn = true;
    protected Vector3 startPosition;
    protected bool hit;
    protected bool dead = false;

    protected readonly int hashDeathParameter = Animator.StringToHash("Death");

    private void Awake()
    {
        characterController2D = GetComponent<CharacterController2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        originalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        dead = false;
    }

    private void Start()
    {
        spriteOriginallyFacesLeft = characterController2D.spriteOriginallyFacesLeft;
        startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (dead)
            return;

        if (Mathf.Approximately(moveVector.y, 0f))
            moveVector.y = 0f;

        if (useGravity && !characterController2D.Grounded)
            moveVector.y -= gravity * Time.deltaTime;

        if ((characterController2D.TouchingWall || (edgeStop && characterController2D.ReachedEdge)) && canTurn)
        {
            canTurn = false;
            StartCoroutine(Turn(.1f));
        }

        if (usePatrolBorders)
        {
            if (rigidbody2D.position.x <= patrolLeft.x + startPosition.x)
            {
                UpdateFacing(false);
                direction = 1;
            }
            if (rigidbody2D.position.x >= patrolRight.x + startPosition.x)
            {
                UpdateFacing(true);
                direction = -1;
            }
        }

        if (characterController2D.Grounded)
            moveVector.x = speed * direction;
        else
            moveVector.x = 0;

        characterController2D.Move(moveVector * Time.deltaTime);
    }

    private void UpdateFacing(bool faceLeft)
    {
        if (faceLeft)
        {
            spriteRenderer.flipX = !spriteOriginallyFacesLeft;
        }
        else
        {
            spriteRenderer.flipX = spriteOriginallyFacesLeft;
        }
    }

    public void ForceTurn()
    {
        canTurn = false;
        StartCoroutine(Turn(.3f));
    }

    protected IEnumerator Turn(float waitTime)
    {
        moveVector.x = 0;
        direction *= -1;
        UpdateFacing(direction == -1);
        yield return new WaitForSeconds(waitTime);
        canTurn = true;
    }

    public void SetMoveVector(Vector2 newMoveVector)
    {
        moveVector = newMoveVector;
    }

    public void Die(Damager damager, Damageable damageable)
    {
        if (flickeringCoroutine != null)
        {
            StopCoroutine(flickeringCoroutine);
            spriteRenderer.color = originalColor;
        }

        flickeringCoroutine = StartCoroutine(Flicker(damageable));

        animator.SetTrigger(hashDeathParameter);

        if (dieAudio != null)
            dieAudio.PlayRandomSound();

        dead = true;

        CameraShaker.Shake(0.15f, 0.3f);
    }

    public void Hit(Damager damager, Damageable damageable)
    {
        if (damageable.CurrentHealth <= 0)
            return;

        moveVector += knockback;

        if (hurtAudio != null)
            hurtAudio.PlayRandomSound();

        if (flickeringCoroutine != null)
        {
            StopCoroutine(flickeringCoroutine);
            spriteRenderer.color = originalColor;
        }

        flickeringCoroutine = StartCoroutine(Flicker(damageable));
        CameraShaker.Shake(0.15f, 0.3f);
    }

    protected IEnumerator Flicker(Damageable damageable)
    {
        float timer = 0f;
        float sinceLastChange = 0.0f;

        Color transparent = originalColor;
        transparent.a = 0.2f;
        int state = 1;

        spriteRenderer.color = transparent;

        while (timer < damageable.invulnerabilityDuration)
        {
            yield return null;
            timer += Time.deltaTime;
            sinceLastChange += Time.deltaTime;
            if (sinceLastChange > flickeringDuration)
            {
                sinceLastChange -= flickeringDuration;
                state = 1 - state;
                spriteRenderer.color = state == 1 ? transparent : originalColor;
            }
        }

        spriteRenderer.color = originalColor;
    }

    public void DisableDamage()
    {
        if (contactDamager != null)
            contactDamager.DisableDamage();
    }

    public void PlayFootStep()
    {
        movementAudio.PlayRandomSound();
    }

    private void OnDrawGizmos()
    {
        if (usePatrolBorders)
        {
            Vector3 gizmoSize = new Vector3(1f, 1f, 1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(startPosition + patrolLeft, gizmoSize);
            Gizmos.DrawWireCube(startPosition + patrolRight, gizmoSize);
        }
    }
}
