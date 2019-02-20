using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBehaviour : MonoBehaviour {

    static Collider2D[] colliderCache = new Collider2D[16];

    protected Vector3 moveVector;
    public Vector3 MoveVector { get { return moveVector; } }
    protected Transform target;
    public Transform Target { get { return target; } }

    public bool spriteFaceLeft = false;

    [Header("Movement")]
    public float speed;
    public float gravity = 10.0f;

    [Header("References")]
    public Bullet projectilePrefab;

    [Header("Patrol Settings")]
    public LayerMask obstacles;
    public float obstacleViewDistance = 0.3f;
    public bool usePatrolBorders;
    public Vector3 patrolLeft = Vector3.zero;
    public Vector3 patrolRight = Vector3.zero;

    [Header("Scanning Settings")]
    [Range(0.0f, 360.0f)]
    public float viewDirection = 0.0f;
    [Range(0.0f, 360.0f)]
    public float viewFOV;
    public float viewDistance;
    public float timeBeforeTargetLost = 3.0f;

    [Header("Melee Attack Data")]
    [EnemyMeleeRangeCheck]
    public float meleeRange = 3.0f;
    public Damager meleeDamager;
    public Damager contactDamager;
    public bool attackDash;
    public Vector2 attackForce;

    [Header("Range Attack Data")]
    public Transform shootingOrigin;
    public float shootAngle = 45.0f;
    public float shootForce = 100.0f;
    public float fireRate = 2.0f;

    [Header("Hurt Data")]
    public Vector2 knockback;
    public float flickeringDuration;

    [Header("Audio")]
    public RandomAudioPlayer shootingAudio;
    public RandomAudioPlayer meleeAttackAudio;
    public RandomAudioPlayer hurtAudio;
    public RandomAudioPlayer dieAudio;
    public RandomAudioPlayer movementAudio;

    protected SpriteRenderer spriteRenderer;
    protected CharacterController2D characterController2D;
    protected new Rigidbody2D rigidbody2D;
    protected new Collider2D collider;
    protected Animator animator;

    protected Vector3 targetShootPosition;
    protected float timeSinceLastTargetView;

    protected float fireTimer = 0.0f;

    protected Vector2 spriteForward;
    protected Bounds localBounds;
    protected Vector3 localDamagerPosition;
    [HideInInspector]
    public Vector3 startPosition;

    protected RaycastHit2D[] raycastHitCache = new RaycastHit2D[8];
    protected ContactFilter2D filter;

    protected Coroutine flickeringCoroutine = null;
    protected Color originalColor;

    protected BulletPool bulletPool;

    protected bool canTurn = true;
    protected bool dead = false;

    protected readonly int hashSpottedParameter = Animator.StringToHash("Spotted");
    protected readonly int hashShootingParameter = Animator.StringToHash("Shooting");
    protected readonly int hashTargetLostParameter = Animator.StringToHash("TargetLost");
    protected readonly int hashMeleeAttackParameter = Animator.StringToHash("MeleeAttack");
    protected readonly int hashHitParameter = Animator.StringToHash("Hit");
    protected readonly int hashDeathParameter = Animator.StringToHash("Death");
    protected readonly int hashGroundedParameter = Animator.StringToHash("Grounded");

    private void Awake()
    {
        characterController2D = GetComponent<CharacterController2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        originalColor = spriteRenderer.color;

        if (projectilePrefab != null)
            bulletPool = BulletPool.GetObjectPool(projectilePrefab.gameObject, 8);

        spriteForward = spriteFaceLeft ? Vector2.left : Vector2.right;
        if (spriteRenderer.flipX)
            spriteForward = -spriteForward;

        if (meleeDamager != null)
            EndAttack();
    }

    private void OnEnable()
    {
        if (meleeDamager != null)
            EndAttack();

        dead = false;
        collider.enabled = true;
    }

    private void Start()
    {
        SceneLinkedSMB<EnemyBehaviour>.Initialise(animator, this);

        localBounds = new Bounds();
        int count = characterController2D.Rigidbody.GetAttachedColliders(colliderCache);
        for (int i = 0; i < count; i++)
        {
            localBounds.Encapsulate(transform.InverseTransformBounds(colliderCache[i].bounds));
        }

        filter = new ContactFilter2D();
        filter.layerMask = obstacles;
        filter.useLayerMask = true;
        filter.useTriggers = false;

        if (meleeDamager)
            localDamagerPosition = meleeDamager.transform.localPosition;

        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (dead)
            return;

        //moveVector.y = Mathf.Max(moveVector.y - gravity * Time.deltaTime, -gravity);

        characterController2D.Move(MoveVector * Time.deltaTime);

        characterController2D.CheckCapsuleEndCollisions();

        UpdateTimers();

        animator.SetBool(hashGroundedParameter, characterController2D.Grounded);
    }
    
    void UpdateTimers()
    {
        if (timeSinceLastTargetView > 0.0f)
            timeSinceLastTargetView -= Time.deltaTime;

        if (fireTimer > 0.0f)
            fireTimer -= Time.deltaTime;
    }

    public void AirVerticalMovement()
    {
        if (Mathf.Approximately(moveVector.y, 0f) || characterController2D.OnCeiling && moveVector.y > 0f)
            moveVector.y = 0f;
        moveVector.y -= gravity * Time.deltaTime;
    }

    public void AirHorizontalMovement()
    {
        moveVector.x = 0;
    }

    public void SetHorizontalSpeed(float horizontalSpeed)
    {
        moveVector.x = horizontalSpeed * spriteForward.x;
    }

    public void NoMovement()
    {
        moveVector = Vector2.zero;
        rigidbody2D.velocity = Vector2.zero;
    }

    public void TurnAround(float speed)
    {
        if (canTurn)
        {
            canTurn = false;
            StartCoroutine(Turn(speed));
        }
    }

    IEnumerator Turn(float speed)
    {
        SetHorizontalSpeed(-speed);
        UpdateFacing();
        yield return new WaitForSeconds(.1f);
        canTurn = true;
    }

    public bool CheckForObstacle(float forwardDistance)
    {
        if (Physics2D.CircleCast(collider.bounds.center, collider.bounds.extents.y - 0.2f, spriteForward, obstacleViewDistance, filter.layerMask.value))
        {
            return true;
        }

        Vector3 castingPosition = (Vector2)(transform.position + localBounds.center) + spriteForward * localBounds.extents.x;
        Debug.DrawLine(castingPosition, castingPosition + Vector3.down * (localBounds.extents.y + 0.2f));

        if (!Physics2D.CircleCast(castingPosition, 0.1f, Vector2.down, localBounds.extents.y + 0.2f, characterController2D.groundedLayerMask.value))
        {
            return true;
        }

        return false;
    }

    public bool CheckWithinPatrolBorders()
    {
        if (rigidbody2D.position.x <= patrolLeft.x + startPosition.x || rigidbody2D.position.x >= patrolRight.x + startPosition.x)
        {
            return false;
        }
        else
            return true;
    }

    public bool CheckGrounded()
    {
        bool grounded = characterController2D.Grounded;

        animator.SetBool(hashGroundedParameter, grounded);

        return grounded;
    }

    public void SetFacingData(int facing)
    {
        if (facing == -1)
        {
            spriteRenderer.flipX = !spriteFaceLeft;
            spriteForward = spriteFaceLeft ? Vector2.right : Vector2.left;
        }
        else if (facing == 1)
        {
            spriteRenderer.flipX = spriteFaceLeft;
            spriteForward = spriteFaceLeft ? Vector2.left : Vector2.right;
        }
    }

    public void SetMoveVector(Vector2 newMoveVector)
    {
        moveVector = newMoveVector;
    }

    public void UpdateFacing()
    {
        bool faceLeft = moveVector.x < 0f;
        bool faceRight = moveVector.x > 0f;

        if (faceLeft)
        {
            SetFacingData(-1);
        }
        else if (faceRight)
        {
            SetFacingData(1);
        }
    }

    public void ScanForPlayer()
    {
        if (!PlayerInput.Instance.HaveControl)
        {
            return;
        }

        Vector3 direction = PlayerCharacter.Instance.transform.position - transform.position;

        if (direction.sqrMagnitude > viewDistance * viewDistance)
        {
            return;
        }

        Vector3 testForward = Quaternion.Euler(0, 0, spriteFaceLeft ? Mathf.Sign(spriteForward.x) * -viewDirection : Mathf.Sign(spriteForward.x) * viewDirection) * spriteForward;

        float angle = Vector3.Angle(testForward, direction);

        if (angle > viewFOV * .5f)
        {
            return;
        }

        target = PlayerCharacter.Instance.transform;
        timeSinceLastTargetView = timeBeforeTargetLost;

        animator.SetTrigger(hashSpottedParameter);
    }

    public void OrientToTarget()
    {
        if (target == null)
        {
            return;
        }

        Vector3 toTarget = target.position - transform.position;

        if (Vector2.Dot(toTarget, spriteForward) < 0)
        {
            SetFacingData(Mathf.RoundToInt(-spriteForward.x));
        }
    }

    public void CheckTargetStillVisible()
    {
        if (target == null)
        {
            return;
        }

        Vector3 toTarget = target.position - transform.position;

        if (toTarget.sqrMagnitude < viewDistance * viewDistance)
        {
            Vector3 testForward = Quaternion.Euler(0, 0, spriteFaceLeft ? -viewDirection : viewDirection) * spriteForward;
            if (spriteRenderer.flipX)
                testForward.x = -testForward.x;

            float angle = Vector3.Angle(testForward, toTarget);

            if (angle <= viewFOV * 0.5f)
                timeSinceLastTargetView = timeBeforeTargetLost;
        }

        if (timeSinceLastTargetView <= 0.0f)
            ForgetTarget();
    }

    public void ForgetTarget()
    {
        animator.SetTrigger(hashTargetLostParameter);
        target = null;
    }

    public void RememberTargetPos()
    {
        if (target == null)
            return;

        targetShootPosition = target.transform.position;
    }

    public void CheckMeleeAttack()
    {
        if (target == null)
        {
            return;
        }

        if ((target.transform.position - transform.position).sqrMagnitude < (meleeRange * meleeRange))
        {
            animator.SetTrigger(hashMeleeAttackParameter);
            meleeAttackAudio.PlayRandomSound();
        }
    }

    public void StartAttack()
    {
        if (spriteRenderer.flipX)
            meleeDamager.transform.localPosition = Vector3.Scale(localDamagerPosition, new Vector3(-1, 1, 1));
        else
            meleeDamager.transform.localPosition = localDamagerPosition;

        meleeDamager.EnableDamage();
        meleeDamager.gameObject.SetActive(true);

        if (attackDash)
            moveVector = new Vector2(spriteForward.x * attackForce.x, attackForce.y);
    }

    public void EndAttack()
    {
        if (meleeDamager != null)
        {
            meleeDamager.gameObject.SetActive(false);
            meleeDamager.DisableDamage();
        }
    }

    public void CheckShootingTimer()
    {
        if (fireTimer > 0.0f)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        animator.SetTrigger(hashShootingParameter);
        shootingAudio.PlayRandomSound();

        fireTimer = fireRate;
    }

    public void Shooting()
    {
        Vector2 force = spriteForward.x > 0 ? Vector2.right.Rotate(shootAngle) : Vector2.left.Rotate(-shootAngle);

        force *= shootForce;

        Vector2 shootPosition = shootingOrigin.transform.localPosition;

        if ((spriteFaceLeft && spriteForward.x > 0) || (!spriteFaceLeft && spriteForward.x > 0))
            shootPosition.x *= -1;

        BulletObject obj = bulletPool.Pop(shootingOrigin.TransformPoint(shootPosition));

        shootingAudio.PlayRandomSound();

        obj.rigidbody2D.velocity = (GetProjectileVelocity(targetShootPosition, shootingOrigin.transform.position));
    }

    private Vector3 GetProjectileVelocity(Vector3 target, Vector3 origin)
    {
        const float projectileSpeed = 30.0f;

        Vector3 velocity = Vector3.zero;
        Vector3 toTarget = target - origin;

        float gSquared = Physics.gravity.sqrMagnitude;
        float b = projectileSpeed * projectileSpeed + Vector3.Dot(toTarget, Physics.gravity);
        float discriminant = b * b - gSquared * toTarget.sqrMagnitude;

        if (discriminant < 0)
        {
            velocity = toTarget;
            velocity.y = 0;
            velocity.Normalize();
            velocity.y = 0.7f;

            velocity *= projectileSpeed;
            return velocity;
        }

        float discRoot = Mathf.Sqrt(discriminant);

        float T_max = Mathf.Sqrt((b + discRoot) * 2f / gSquared);

        float T_lowEnergy = Mathf.Sqrt(Mathf.Sqrt(toTarget.sqrMagnitude * 4f / gSquared));

        float T_min = Mathf.Sqrt((b - discRoot) * 2f / gSquared);

        float T = 0;

        int shotType = 1;

        switch (shotType)
        {
            case 0:
                T = T_max;
                break;
            case 1:
                T = T_lowEnergy;
                break;
            case 2:
                T = T_min;
                break;
            default:
                break;
        }

        velocity = toTarget / T - Physics.gravity * T / 2f;

        return velocity;
    }

    public void Death()
    {
        Destroy(this.gameObject);
    }

    public void Die(Damager damager, Damageable damageable)
    {
        Vector2 throwVector = new Vector2(0, 5.0f);
        Vector2 damagerToThis = damager.transform.position - transform.position;

        throwVector.x = Mathf.Sign(damagerToThis.x) * 4.0f;
        SetMoveVector(throwVector);

        animator.SetTrigger(hashDeathParameter);

        if (dieAudio != null)
            dieAudio.PlayRandomSound();

        dead = true;
        collider.enabled = false;

        CameraShaker.Shake(0.15f, 0.3f);
    }

    public void Hit(Damager damager, Damageable damageable)
    {
        if (damageable.CurrentHealth <= 0)
            return;

        animator.SetTrigger(hashHitParameter);

        //Vector2 throwVector = new Vector2(0, 5.0f);
        //Vector2 damagerToThis = damager.transform.position - transform.position;

        //throwVector.x = Mathf.Sign(damagerToThis.x) * 2.0f;
        //moveVector = throwVector;

        moveVector = Vector2.zero;
        moveVector = new Vector2(0, knockback.y);

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
        if (meleeDamager != null)
            meleeDamager.DisableDamage();
        if (contactDamager != null)
            contactDamager.DisableDamage();
    }

    public void PlayMovementAudio()
    {
        movementAudio.PlayRandomSound();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 forward = spriteFaceLeft ? Vector2.left : Vector2.right;
        forward = Quaternion.Euler(0, 0, spriteFaceLeft ? -viewDirection : viewDirection) * forward;

        if (GetComponent<SpriteRenderer>().flipX)
            forward.x = -forward.x;

        Vector3 endpoint = transform.position + (Quaternion.Euler(0, 0, viewFOV * 0.5f) * forward);

        Handles.color = new Color(0, 1.0f, 0, 0.2f);
        Handles.DrawSolidArc(transform.position, -Vector3.forward, (endpoint - transform.position).normalized, viewFOV, viewDistance);

        Handles.color = new Color(1.0f, 0, 0, 0.1f);
        Handles.DrawSolidDisc(transform.position, Vector3.back, meleeRange);
    }
#endif
}

public class EnemyMeleeRangeCheckAttribute : PropertyAttribute
{

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnemyMeleeRangeCheckAttribute))]
public class EnemyMeleeRangePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty viewRangeProperty = property.serializedObject.FindProperty("viewDistance");
        if (viewRangeProperty.floatValue < property.floatValue)
        {
            Rect pos = position;
            pos.height = 30;
            EditorGUI.HelpBox(pos, "Melee range is bigger than View distance. Note enemies only attack if target is in their view range first", MessageType.Warning);
            position.y += 30;
        }

        EditorGUI.PropertyField(position, property, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty viewRangeProperty = property.serializedObject.FindProperty("viewDistance");
        if (viewRangeProperty.floatValue < property.floatValue)
            return base.GetPropertyHeight(property, label) + 30;
        else
            return base.GetPropertyHeight(property, label);
    }
}
#endif