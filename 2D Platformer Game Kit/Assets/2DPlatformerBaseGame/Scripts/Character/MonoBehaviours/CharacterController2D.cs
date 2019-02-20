using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    public LayerMask groundedLayerMask;
    public float groundedRaycastDistance = 0.1f;
    
    new CapsuleCollider2D collider;
    new Rigidbody2D rigidbody;
    ContactFilter2D groundContactFilter;
    
    RaycastHit2D[] groundHitResults = new RaycastHit2D[5];
    RaycastHit2D[] groundFoundHits = new RaycastHit2D[3];
    Collider2D[] groundColliders = new Collider2D[3];
    Vector2[] groundRaycastPositions = new Vector2[3];
    Vector2 previousPosition;
    Vector2 currentPosition;
    Vector2 nextMovement;

    public bool Grounded { get; protected set; }
    public bool OnCeiling { get; protected set; }
    public bool ReachedEdge { get; protected set; }
    public Vector2 Velocity { get; protected set; }
    public Rigidbody2D Rigidbody { get { return rigidbody; } }
    public Collider2D[] GroundColliders { get { return groundColliders; } }
    public ContactFilter2D GroundContactFilter { get { return groundContactFilter; } }

    // Wall Slide

    public bool useWallDetect = true;
    public bool detectBothSides = false;
    public bool spriteOriginallyFacesLeft;
    public SpriteRenderer spriteRenderer;
    public LayerMask wallLayerMask;
    public float wallRaycastDistance = 0.1f;

    ContactFilter2D wallContactFilter;

    RaycastHit2D[] wallHitResults = new RaycastHit2D[5];
    RaycastHit2D[] wallFoundHits = new RaycastHit2D[3];
    Collider2D[] wallColliders = new Collider2D[3];
    Vector2[] wallRaycastPositions = new Vector2[3];

    public bool TouchingWall { get; protected set; }
    public Collider2D[] WallColliders { get { return wallColliders; } }
    public ContactFilter2D WallContactFilter { get { return wallContactFilter; } }

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<CapsuleCollider2D>();

        currentPosition = rigidbody.position;
        previousPosition = rigidbody.position;

        groundContactFilter.layerMask = groundedLayerMask;
        groundContactFilter.useLayerMask = true;
        groundContactFilter.useTriggers = false;

        wallContactFilter.layerMask = wallLayerMask;
        wallContactFilter.useLayerMask = true;
        wallContactFilter.useTriggers = false;

        Physics2D.queriesStartInColliders = false;
    }

    void FixedUpdate()
    {
        previousPosition = rigidbody.position;
        currentPosition = previousPosition + nextMovement;
        Velocity = (currentPosition - previousPosition) / Time.deltaTime;
        rigidbody.MovePosition(currentPosition);
        nextMovement = Vector2.zero;

        CheckCapsuleEndCollisions();
        CheckCapsuleEndCollisions(false);
        if (useWallDetect)
        {
            if (detectBothSides)
                CheckWallCollisions(true);
            CheckWallCollisions();
        }
    }

    public void Move(Vector2 movement)
    {
        nextMovement += movement;
    }

    public void Teleport(Vector2 position)
    {
        Vector2 delta = position - currentPosition;
        previousPosition += delta;
        currentPosition = position;
        rigidbody.MovePosition(position);
    }

    public void CheckWallCollisions(bool otherDirection = false)
    {
        Vector2 raycastDirection;
        Vector2 raycastStart;
        float raycastDistance;
        bool faceLeft = spriteRenderer.flipX != spriteOriginallyFacesLeft ? true : false;
        if (otherDirection)
            faceLeft = !faceLeft;

        if (collider == null)
        {
            raycastStart = rigidbody.position + Vector2.up;
            raycastDistance = 1f + wallRaycastDistance;

            if (faceLeft)
            {
                raycastDirection = Vector2.left;

                wallRaycastPositions[0] = raycastStart + Vector2.up * 0.4f;
                wallRaycastPositions[1] = raycastStart;
                wallRaycastPositions[2] = raycastStart + Vector2.down * 0.4f;
            }
            else
            {
                raycastDirection = Vector2.right;

                wallRaycastPositions[0] = raycastStart + Vector2.up * 0.4f;
                wallRaycastPositions[1] = raycastStart;
                wallRaycastPositions[2] = raycastStart + Vector2.down * 0.4f;
            }
        }
        else
        {
            raycastStart = rigidbody.position + collider.offset;
            raycastDistance = collider.size.x * .5f + wallRaycastDistance;

            if (faceLeft)
            {
                raycastDirection = Vector2.left;
                Vector2 raycastStartLeftCenter = raycastStart + Vector2.left * (collider.size.y * 0.5f - collider.size.x * 0.5f);

                wallRaycastPositions[0] = raycastStartLeftCenter + Vector2.up * collider.size.x * 0.5f;
                wallRaycastPositions[1] = raycastStartLeftCenter;
                wallRaycastPositions[2] = raycastStartLeftCenter + Vector2.down * collider.size.x * 0.5f;
            }
            else
            {
                raycastDirection = Vector2.right;
                Vector2 raycastStartLeftCenter = raycastStart + Vector2.right * (collider.size.y * 0.5f - collider.size.x * 0.5f);

                wallRaycastPositions[0] = raycastStartLeftCenter + Vector2.up * collider.size.x * 0.5f;
                wallRaycastPositions[1] = raycastStartLeftCenter;
                wallRaycastPositions[2] = raycastStartLeftCenter + Vector2.down * collider.size.x * 0.5f;
            }
        }

        for (int i = 0; i < wallRaycastPositions.Length; i++)
        {
            int count = Physics2D.Raycast(wallRaycastPositions[i], raycastDirection, wallContactFilter, wallHitResults, raycastDistance);

            wallFoundHits[i] = count > 0 ? wallHitResults[0] : new RaycastHit2D();
            wallColliders[i] = wallFoundHits[i].collider;
        }

        Vector2 wallNormal = Vector2.zero;
        int hitCount = 0;

        for (int i = 0; i < wallFoundHits.Length; i++)
        {
            if (wallFoundHits[i].collider != null)
            {
                wallNormal += wallFoundHits[i].normal;
                hitCount++;
            }
        }

        if (hitCount > 1)
        {
            wallNormal.Normalize();
        }
        else
        {
            TouchingWall = false;
            return;
        }

        Vector2 relativeVelocity = rigidbody.velocity;

        for (int i = 0; i < wallColliders.Length; i++)
        {
            if (wallColliders[i] == null)
            {
                continue;
            }

            MovingPlatform movingPlatform;

            if (PhysicsHelper.TryGetMovingPlatform(wallColliders[i], out movingPlatform))
            {
                relativeVelocity -= movingPlatform.Velocity / Time.deltaTime;
                break;
            }
        }

        if (Mathf.Approximately(wallNormal.x, 0f) && Mathf.Approximately(wallNormal.y, 0f))
        {
            TouchingWall = false;
        }
        else
        {
            if (faceLeft)
            {
                TouchingWall = (relativeVelocity.x >= 0f);

                if (collider != null)
                {
                    if (wallColliders[1] != null)
                    {
                        float colliderLeftWidth = rigidbody.position.x + collider.offset.x - collider.size.x * 0.5f;
                        float middleHitWidth = wallFoundHits[1].point.x;
                        TouchingWall &= (middleHitWidth > colliderLeftWidth - wallRaycastDistance);
                    }
                }
            }
            else
            {
                TouchingWall = (relativeVelocity.x <= 0f);

                if (collider != null)
                {
                    if (wallColliders[1] != null)
                    {
                        float colliderRightWidth = rigidbody.position.x + collider.offset.x + collider.size.x * 0.5f;
                        float middleHitWidth = wallFoundHits[1].point.x;
                        TouchingWall &= (middleHitWidth < colliderRightWidth + wallRaycastDistance);
                    }
                }
            }
        }

        for (int i = 0; i < wallHitResults.Length; i++)
        {
            wallHitResults[i] = new RaycastHit2D();
        }
    }

    public void CheckCapsuleEndCollisions(bool bottom = true)
    {
        Vector2 raycastDirection;
        Vector2 raycastStart;
        float raycastDistance;

        if (collider == null)
        {
            raycastStart = rigidbody.position + Vector2.up;
            raycastDistance = 1f + groundedRaycastDistance;

            if (bottom)
            {
                raycastDirection = Vector2.down;

                groundRaycastPositions[0] = raycastStart + Vector2.left * 0.4f;
                groundRaycastPositions[1] = raycastStart;
                groundRaycastPositions[2] = raycastStart + Vector2.right * 0.4f;
            }
            else
            {
                raycastDirection = Vector2.up;

                groundRaycastPositions[0] = raycastStart + Vector2.left * 0.4f;
                groundRaycastPositions[1] = raycastStart;
                groundRaycastPositions[2] = raycastStart + Vector2.right * 0.4f;
            }
        }
        else
        {
            raycastStart = rigidbody.position + collider.offset;
            raycastDistance = collider.size.x * .5f + groundedRaycastDistance * 2f;

            if (bottom)
            {
                raycastDirection = Vector2.down;
                Vector2 raycastStartBottomCenter = raycastStart + Vector2.down * (collider.size.y * 0.5f - collider.size.x * 0.5f);

                groundRaycastPositions[0] = raycastStartBottomCenter + Vector2.left * collider.size.x * 0.5f;
                groundRaycastPositions[1] = raycastStartBottomCenter;
                groundRaycastPositions[2] = raycastStartBottomCenter + Vector2.right * collider.size.x * 0.5f;
            }
            else
            {
                raycastDirection = Vector2.up;
                Vector2 raycastStartTopCenter = raycastStart + Vector2.up * (collider.size.y * 0.5f - collider.size.x * 0.5f);

                groundRaycastPositions[0] = raycastStartTopCenter + Vector2.left * collider.size.x * 0.5f;
                groundRaycastPositions[1] = raycastStartTopCenter;
                groundRaycastPositions[2] = raycastStartTopCenter + Vector2.right * collider.size.x * 0.5f;
            }
        }

        for (int i = 0; i < groundRaycastPositions.Length; i++)
        {
            int count = Physics2D.Raycast(groundRaycastPositions[i], raycastDirection, groundContactFilter, groundHitResults, raycastDistance);

            if (bottom)
            {
                groundFoundHits[i] = count > 0 ? groundHitResults[0] : new RaycastHit2D();
                groundColliders[i] = groundFoundHits[i].collider;
            }
            else
            {
                OnCeiling = false;

                for (int j = 0; j < groundHitResults.Length; j++)
                {
                    if (groundHitResults[j].collider != null)
                    {
                        if (!PhysicsHelper.ColliderHasPlatformEffector(groundHitResults[j].collider))
                        {
                            OnCeiling = true;
                        }
                    }
                }
            }
        }

        if (bottom)
        {
            Vector2 groundNormal = Vector2.zero;
            int hitCount = 0;

            for (int i = 0; i < groundFoundHits.Length; i++)
            {
                if (groundFoundHits[i].collider != null)
                {
                    groundNormal += groundFoundHits[i].normal;
                    hitCount++;
                }
            }

            if (hitCount > 0)
            {
                groundNormal.Normalize();
            }

            if (hitCount < 3 && hitCount > 0)
                ReachedEdge = true;
            else
                ReachedEdge = false;

            Vector2 relativeVelocity = Velocity;

            for (int i = 0; i < groundColliders.Length; i++)
            {
                if (groundColliders[i] == null)
                    continue;

                MovingPlatform movingPlatform;

                if (PhysicsHelper.TryGetMovingPlatform(groundColliders[i], out movingPlatform))
                {
                    relativeVelocity -= movingPlatform.Velocity / Time.deltaTime;
                    break;
                }
            }

            if (Mathf.Approximately(groundNormal.x, 0f) && Mathf.Approximately(groundNormal.y, 0f))
            {
                Grounded = false;
            }
            else
            {
                Grounded = (relativeVelocity.y <= 0f);

                if (collider != null)
                {
                    if (groundColliders[1] != null)
                    {
                        float colliderBottomHeight = rigidbody.position.y + collider.offset.y - collider.size.y * 0.5f;
                        float middleHitHeight = groundFoundHits[1].point.y;
                        Grounded &= (middleHitHeight < colliderBottomHeight + groundedRaycastDistance);
                    }
                }
            }
        }

        for (int i = 0; i < groundHitResults.Length; i++)
        {
            groundHitResults[i] = new RaycastHit2D();
        }
    }
}
