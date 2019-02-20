using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A very rough follower script. Includes relics of several attempts to get it to work properly. Currently, very hack-ish.
/// </summary>

public class BoyController : MonoBehaviour
{
    static protected BoyController boyInstance;
    static public BoyController BoyInstance { get { return boyInstance; } }

    public float speed = 10f;
    public float distanceFromPlayer = 2f;
    public float maxDistance = 20f;
    public float followDelay = 0.5f;
    public float gravity = 10f;
    public string triggerTag ="";
    public bool spriteOriginallyFacesLeft = false;

    public RandomAudioPlayer footstepAudioPlayer;
    public RandomAudioPlayer landingAudioPlayer;

    protected CharacterController2D characterController2D;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected new Rigidbody2D rigidbody2D;
    protected new CapsuleCollider2D collider;
    protected Vector2 moveVector = Vector2.zero;
    protected bool grounded = false;
    protected Vector2 jumpLocation = Vector2.zero;
    protected Vector2 landLocation = Vector2.zero;
    protected bool hasJumpLoc = false;
    protected bool hasLandLoc = false;
    protected bool jumped = false;
    protected Vector3 distanceFromPlayerVector;
    
    protected bool inPause = false;

    protected readonly int hashHorizontalSpeedParameter = Animator.StringToHash("HorizontalSpeed");
    protected readonly int hashVerticalSpeedParameter = Animator.StringToHash("VerticalSpeed");
    protected readonly int hashGroundedParameter = Animator.StringToHash("Grounded");

    private void Awake()
    {
        boyInstance = this;
        characterController2D = GetComponent<CharacterController2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider = GetComponent<CapsuleCollider2D>();
    }

    private void Start()
    {
        SceneLinkedSMB<BoyController>.Initialise(animator, this);

        distanceFromPlayerVector = new Vector3(distanceFromPlayer, 0);

        transform.position = PlayerCharacter.Instance.transform.position - distanceFromPlayerVector;
    }

    private void FixedUpdate()
    {
        Gravity();
        CheckGrounded();
        RubberBand();

        if (CheckWithinDistance())
        {
            moveVector.x = 0;
            if (grounded)
            {
                if (PlayerCharacter.Instance.CheckForJumpInput())
                {
                    Jump();
                }
            }
        }
        else
        {
            StartCoroutine(MoveToPlayer());
            if (characterController2D.TouchingWall)
                Jump();
            else if (characterController2D.ReachedEdge && PlayerCharacter.Instance.transform.position.y > transform.position.y)
                Jump();
        }

        characterController2D.Move(moveVector * Time.deltaTime);
        animator.SetFloat(hashHorizontalSpeedParameter, moveVector.x);
        animator.SetFloat(hashVerticalSpeedParameter, moveVector.x);

        UpdateFacing();
    }

    private void RubberBand()
    {
        float x = Mathf.Abs(transform.position.x - PlayerCharacter.Instance.transform.position.x);
        float y = Mathf.Abs(transform.position.y - PlayerCharacter.Instance.transform.position.y);
        float distance = Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
        if (distance > maxDistance)
            Respawn(PlayerCharacter.Instance.transform.position);
    }

    public void Respawn(Vector3 playerLoc)
    {
        GameObjectTeleporter.Teleport(gameObject, new Vector2(playerLoc.x - distanceFromPlayer, playerLoc.y));
    }

    #region COROUTINES

    IEnumerator MoveWithPlayer()
    {
        yield return new WaitForSeconds(followDelay);
        moveVector = PlayerCharacter.Instance.GetMoveVector();
    }

    IEnumerator MoveToPlayer()
    {
        yield return new WaitForSeconds(followDelay);
        float followLoc = PlayerCharacter.Instance.transform.position.x + -PlayerCharacter.Instance.GetFacing() * distanceFromPlayer;
        moveVector.x = (followLoc - transform.position.x) * speed;
        if (Mathf.Approximately(moveVector.x, 0f))
        {
            moveVector.x = 0;
        }
    }

    IEnumerator MoveToJumpLocation()
    {
        moveVector.x = jumpLocation.x - transform.position.x;
        yield return null;
    }

    IEnumerator MoveToLandLocation()
    {
        moveVector.x = landLocation.x - transform.position.x;
        yield return null;
    }

    #endregion

    private void Jump()
    {
        moveVector.y = PlayerCharacter.Instance.jumpSpeed;
    }

    private void LocationJump()
    {
        Jump();
        jumpLocation = Vector2.zero;
        jumped = true;
    }

    private void Gravity()
    {
        moveVector.y -= gravity * Time.deltaTime;
    }

    private bool CheckGrounded()
    {
        grounded = characterController2D.Grounded;
        animator.SetBool(hashGroundedParameter, grounded);

        return grounded;
    }

    private bool CheckWithinDistance()
    {
        if (Mathf.Approximately(transform.position.x + distanceFromPlayer * PlayerCharacter.Instance.GetFacing(), PlayerCharacter.Instance.transform.position.x))
            return true;
        if (Mathf.Abs(transform.position.x - PlayerCharacter.Instance.transform.position.x) > distanceFromPlayer)
            return false;
        else
            return true;
    }

    private void NoJumpLoc()
    {
        hasJumpLoc = false;
    }

    private void NoLandLoc()
    {
        hasLandLoc = false;
    }
    

    #region FACING

    private void ForceFacing(bool faceLeft)
    {
        if (faceLeft)
            spriteRenderer.flipX = !spriteOriginallyFacesLeft;
        else
            spriteRenderer.flipX = spriteOriginallyFacesLeft;
    }

    private void UpdateFacing()
    {
        bool faceLeft = moveVector.x < 0f;
        bool faceRight = moveVector.x > 0f;

        if (faceLeft)
            spriteRenderer.flipX = !spriteOriginallyFacesLeft;
        else if (faceRight)
            spriteRenderer.flipX = spriteOriginallyFacesLeft;
    }

    public float GetFacing()
    {
        return spriteRenderer.flipX != spriteOriginallyFacesLeft ? -1f : 1f;
    }

    #endregion
}
