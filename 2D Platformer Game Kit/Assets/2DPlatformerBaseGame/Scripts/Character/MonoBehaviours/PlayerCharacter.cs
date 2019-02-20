using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Controls all of the player's functionality
/// </summary>

[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(Animator))]

public class PlayerCharacter : MonoBehaviour
{
    #region VARIABLES

    static protected PlayerCharacter instance; // Creates a protected singleton (only one instance) of the player, as there is only one.
    static public PlayerCharacter Instance { get { return instance; } } // Create a public singleton of the player that returns the protected one. Allows all other scripts to access the player with PlayerCharacter.Instance

    public InventoryController InventoryController { get { return inventoryController; } } // Only allow this script to directly change the inventoryController.
    
    // References assigned in the inspector
    public SpriteRenderer spriteRenderer; // Shows the sprite.
    public Damageable damageable; // Tracks health and processes damage.
    public Damager meleeDamager; // Deals damage during melee attacks.
    public Transform facingLeftBulletSpawnPoint; // Empty gameobject that is the location bullets spawn on the left.
    public Transform facingRightBulletSpawnPoint; // Same thing except on the right.
    public BulletPool bulletPool; // Holds settings and values for ranged bullets.
    public Transform cameraFollowTarget; // Empty gameobject the camera follows.
    public BoyController boy; // The optional boy who will follow the player and offer advice.

    // Movement Settings
    public float maxSpeed = 10f;
    public float groundAcceleration = 100f;
    public float groundDeceleration = 100f;
    [Tooltip("Reduces the speed the player pushes blocks. 0.3 = 30% normal speed")]
    [Range(0f, 1f)] public float pushingSpeedProportion;

    // Airborne Settings
    [Range(0f, 1f)] public float airborneAccelProportion;
    [Range(0f, 1f)] public float airborneDecelProportion;
    public float gravity = 50f;
    public float jumpSpeed = 20f;
    [Tooltip("Causes a low jump height when the jump key is released early.")]
    public float jumpAbortSpeedReduction = 100f;

    // Wallslide Settings
    public bool wallSlideOn = false;
    public float wallSlideUpGravity = 70f;
    public float wallSlideDownGravity = 5f;
    public float wallSlideJumpX = 20f;
    public float wallSlideJumpY = 20f;
    [Tooltip("Reduces the amount of airborne deceleration to allow for a longer, more realistic wall jump arc.")]
    [Range(0f, 1f)] public float wallSlideAirborneDecelProportion;
    public float wallSlideTimeoutDuration = 3f;
    public bool canWallSlideUp = false;
    public float wallSlideUpSpeed = 0f;

    // Hurt Settings
    [Range(minHurtJumpAngle, maxHurtJumpAngle)] public float hurtJumpAngle = 45f;
    public float hurtJumpSpeed = 5f;
    [Tooltip("The rate at which the player flickers when damaged, i.e. the time between each flicker.")]
    public float flickeringDuration = 0.1f;

    // Melee Settings
    public bool meleeAttackOn = false;
    public float meleeAttackDashSpeed = 5f;
    public bool dashWhileAirborne = false;
    [Tooltip("Time delay between the triggering of melee attack and damage processing. Used to sync with player animations.")]
    [Range(0f, 0.379f)]public float meleeDamagerDelay = 0.06f;

    // Ranged Settings
    public bool rangedAttackOn = false;
    public float shotsPerSecond = 1f;
    public float bulletSpeed = 5f;
    public float holdingGunTimeoutDuration = 10f;
    [Tooltip("Whether or not the bullet spawn point is animated on the right or left side in cases where the gun moves during firing animations. Will set unanimated side to be the opposite of the animated.")]
    public bool rightBulletSpawnPointAnimated = true;

    // Audio Settings
    public RandomAudioPlayer footstepAudioPlayer;
    public RandomAudioPlayer landingAudioPlayer;
    public RandomAudioPlayer hurtAudioPlayer;
    public RandomAudioPlayer meleeAttackAudioPlayer;
    public RandomAudioPlayer rangedAttackAudioPlayer;

    // Camera Follow Settings
    public float cameraHorizontalFacingOffset = 2f;
    public float cameraHorizontalSpeedOffset = 0.2f;
    public float cameraVerticalInputOffset = 2f;
    public float maxHorizontalDeltaDampTime = 0.4f;
    public float maxVerticalDeltaDampTime = 0.6f;
    public float verticalCameraOffsetDelay = 1f;

    // Misc Settings
    public string triggerTag; // Used for event manager in case you want to set up events triggered by the player.
    public bool spriteOriginallyFacesLeft;

    // Protected Variables
    protected CharacterController2D characterController2D;
    protected Animator animator;
    protected new CapsuleCollider2D collider;
    protected Vector2 moveVector;
    protected List<Pushable> currentPushables = new List<Pushable>(4);
    protected Pushable currentPushable;
    protected float tanHurtJumpAngle;
    protected WaitForSeconds flickeringWait;
    protected Coroutine flickerCoroutine;
    protected Transform currentBulletSpawnPoint;
    protected float shotSpawnGap;
    protected WaitForSeconds shotSpawnWait;
    protected Coroutine shootingCoroutine;
    protected float nextShotTime;
    protected bool isFiring;
    protected float shotTimer;
    protected float rangedAttackTimeRemaining;
    protected float wallSlideTimeRemaining;
    protected float wallSlideCooldownTimeRemaining;
    protected bool canSlide = true;
    protected bool slideCooldown = false;
    protected TileBase currentSurface;
    protected float camFollowHorizontalSpeed;
    protected float camFollowVerticalSpeed;
    protected float verticalCameraOffsetTimer;
    protected InventoryController inventoryController;
    protected bool hasBoy = true;

    protected Checkpoint lastCheckpoint = null;
    protected Vector2 startingPosition = Vector2.zero;
    protected bool startingFacingLeft = false;

    protected bool inPause = false;

    // Animator Parameters. Turns parameter names to hash id's to access them with ints instead of strings. Searching strings is more expensive. Not terribly important for a game of this size.
    protected readonly int hashHorizontalSpeedParameter = Animator.StringToHash("HorizontalSpeed");
    protected readonly int hashVerticalSpeedParameter = Animator.StringToHash("VerticalSpeed");
    protected readonly int hashGroundedParameter = Animator.StringToHash("Grounded");
    // protected readonly int hashCrouchingPara = Animator.StringToHash("Crouching");
    protected readonly int hashPushingPara = Animator.StringToHash("Pushing");
    protected readonly int hashTimeoutParameter = Animator.StringToHash("Timeout");
    protected readonly int hashRespawnParameter = Animator.StringToHash("Respawn");
    protected readonly int hashDeadParameter = Animator.StringToHash("Dead");
    protected readonly int hashHurtParameter = Animator.StringToHash("Hurt");
    protected readonly int hashForcedRespawnParameter = Animator.StringToHash("ForcedRespawn");
    protected readonly int hashMeleeAttackParameter = Animator.StringToHash("MeleeAttack");
    protected readonly int hashRangedAttackParameter = Animator.StringToHash("RangedAttack");
    protected readonly int hashWallSlideParameter = Animator.StringToHash("WallSlide");

    // Constant Variables
    protected const float minHurtJumpAngle = 0.001f;
    protected const float maxHurtJumpAngle = 89.999f;
    protected const float groundStickVelocityMultiplier = 5f;

    // Hidden Public Variables
    [HideInInspector]
    public Shield shield;
    [HideInInspector]
    public int swordDurability = 0;
    [HideInInspector]
    public int gunAmmo = 0;
    [HideInInspector]
    public bool canBoost = true;

    #endregion

    void Awake()
    {
        // Assigning variables
        instance = this; // assign player singleton variable to this script.
        characterController2D = GetComponent<CharacterController2D>();
        animator = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider2D>();
        shield = GetComponent<Shield>();
        inventoryController = GetComponent<InventoryController>();
        damageable = GetComponent<Damageable>();

        currentBulletSpawnPoint = spriteOriginallyFacesLeft ? facingLeftBulletSpawnPoint : facingRightBulletSpawnPoint; // If the sprite is facing left, set current bullet spawn to left. Right, set it to right.
    }

    void Start()
    {
        meleeDamager.DisableDamage(); // Turn off melee damager, as it should only be on when using a melee attack.

        // Hurt jump angle calculations.
        hurtJumpAngle = Mathf.Clamp(hurtJumpAngle, minHurtJumpAngle, maxHurtJumpAngle); 
        tanHurtJumpAngle = Mathf.Tan(Mathf.Deg2Rad * hurtJumpAngle); 
        
        // Set up ranged attack variables.
        shotSpawnGap = 1f / shotsPerSecond;
        nextShotTime = Time.time;
        shotSpawnWait = new WaitForSeconds(shotSpawnGap);
        
        // Camera follow speed calculations.
        if (!Mathf.Approximately(maxHorizontalDeltaDampTime, 0f))
        {
            float maxHorizontalDelta = maxSpeed * cameraHorizontalSpeedOffset + cameraHorizontalFacingOffset;
            camFollowHorizontalSpeed = maxHorizontalDelta / maxHorizontalDeltaDampTime;
        }
        if (!Mathf.Approximately(maxVerticalDeltaDampTime, 0f))
        {
            float maxVerticalDelta = cameraVerticalInputOffset;
            camFollowVerticalSpeed = maxVerticalDelta / maxVerticalDeltaDampTime;
        }

        SceneLinkedSMB<PlayerCharacter>.Initialise(animator, this); // Tells state machine to use this script to create and set up each state machine behaviour.

        flickeringWait = new WaitForSeconds(flickeringDuration); // Set up flicker WaitForSeconds, so it doesn't have to be set up multiple times.

        startingPosition = transform.position; // Set starting position.
        startingFacingLeft = (GetFacing() < 0.0f); // Set whether player started facing left or right.


        // Set available abilities at start.
        if (!wallSlideOn)
            DisableWallSlide();
        else
            EnableWallSlide();

        if (!meleeAttackOn)
            DisableMeleeAttack();
        else
            EnableMeleeAttack();

        if (!rangedAttackOn)
            DisableRangedAttack();
        else
            EnableRangedAttack();
    }

    void Update()
    {
        // Check for pause
        if (PlayerInput.Instance.Pause.Down)
        {
            if (!inPause) // If not already paused...
            {
                if (ScreenFader.IsFading)
                    return;

                PlayerInput.Instance.ReleaseControl(false); // Stop receiving player input i.e. the player can't do anything.
                PlayerInput.Instance.Pause.GainControl(); // Gain control of the pause input in order to be able to unpause. Could add more controls here such as menu navigation.
                inPause = true; // To differentiate between paused/unpaused.
                Time.timeScale = 0; // Stop game time, so everything in the level "pauses".
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("UIMenus", UnityEngine.SceneManagement.LoadSceneMode.Additive); // Load pause menu scene to be overlaid.
            }
            else
            {
                Unpause(); // Starts unpause coroutine.
            }
        }
    }

    void FixedUpdate()
    {
        characterController2D.Move(moveVector * Time.deltaTime); // Move character by the moveVector calculated this update.
        animator.SetFloat(hashHorizontalSpeedParameter, moveVector.x); // Set animator parameter to change animation for left and right movement.
        animator.SetFloat(hashVerticalSpeedParameter, moveVector.y); // Set animator parameter to change jump animation.
        UpdateBulletSpawnPointPositions(); // Update the position of the unanimated bullet spawn point.
        UpdateCameraFollowTargetPosition(); // Updates the camera follow target to ensure the camera is moving correctly.
    }

    #region TRIGGERS

    private void OnTriggerEnter2D(Collider2D other) // When player enters a collider that is a trigger...
    {
        if (other.tag == "Heart" && damageable.CurrentHealth != damageable.startingHealth) // If the trigger you collide with is a heart and you don't have full health...
        {
            Destroy(other.transform.parent.gameObject); // Remove the heart.
            if (damageable.CurrentHealth == damageable.startingHealth - 1) // If only missing 1 health...
                damageable.GainHealth(1); // Add 1 health.
            else
                damageable.GainHealth(2); // Add 2 health.
        }
        else
        {
            Pushable pushable = other.GetComponent<Pushable>(); // Assign the trigger gameobject's pushable component. Would only have one if the gameobject is pushable
            if (pushable != null) // If the object has a pushable component...
                currentPushables.Add(pushable); // Add pushable object to list of objects the player is currently pushing
        }
    }

    private void OnTriggerExit2D(Collider2D other) // For when the player exits a trigger collider, follow the same procedure as the enter, and remove it from the list if it was a pushable
    {
        Pushable pushable = other.GetComponent<Pushable>();
        if (pushable != null)
        {
            if (currentPushables.Contains(pushable))
                currentPushables.Remove(pushable);
        }
    }

    #endregion

    #region PAUSE

    public void Unpause()
    {
        if (Time.timeScale > 0) // Check to see if time is already progressing normally, i.e. game is already unpaused
            return; // If it is, ignore this

        StartCoroutine(UnpauseCoroutine()); // Begin unpause coroutine
    }

    protected IEnumerator UnpauseCoroutine()
    {
        Time.timeScale = 1; // Resumes normal time progression
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("UIMenus"); // Unloads pause menu
        PlayerInput.Instance.GainControl(); // Player gains control once again
        yield return new WaitForFixedUpdate(); // Waits until next fixed update cycle is called so the player can't spam pause/unpause
        yield return new WaitForEndOfFrame(); // Waits until the end of the frame
        inPause = false; // Reenable the ability to pause
    }

    #endregion

    #region CAMERA FOLLOW

    protected void UpdateCameraFollowTargetPosition() // Uses camera variables to move the camera target for seeing ahead, deadzone, smoothing, etc.
    {
        float newLocalPosX;
        float newLocalPosY = 0f;

        float desiredLocalPosX = (spriteOriginallyFacesLeft ^ spriteRenderer.flipX ? -1f : 1f) * cameraHorizontalFacingOffset;
        desiredLocalPosX += moveVector.x * cameraHorizontalSpeedOffset;
        if (Mathf.Approximately(camFollowHorizontalSpeed, 0f))
        {
            newLocalPosX = desiredLocalPosX;
        }
        else
        {
            newLocalPosX = Mathf.Lerp(cameraFollowTarget.localPosition.x, desiredLocalPosX, camFollowHorizontalSpeed * Time.deltaTime);
        }
        bool moveVertically = false;
        if (!Mathf.Approximately(PlayerInput.Instance.Vertical.Value, 0f))
        {
            verticalCameraOffsetTimer += Time.deltaTime;

            if (verticalCameraOffsetTimer >= verticalCameraOffsetDelay)
            {
                moveVertically = true;
            }
        }
        else
        {
            moveVertically = true;
            verticalCameraOffsetTimer = 0f;
        }

        if (moveVertically)
        {
            float desiredLocalPosY = PlayerInput.Instance.Vertical.Value * cameraVerticalInputOffset;
            if (Mathf.Approximately(camFollowVerticalSpeed, 0f))
            {
                newLocalPosY = desiredLocalPosY;
            }
            else
            {
                newLocalPosY = Mathf.MoveTowards(cameraFollowTarget.localPosition.y, desiredLocalPosY, camFollowVerticalSpeed * Time.deltaTime);
            }
        }

        cameraFollowTarget.localPosition = new Vector2(newLocalPosX, newLocalPosY);
    }

    #endregion

    #region BOY FOLLOW

    public void GainBoy() // Can be called to gain follower.
    {
        hasBoy = true;
    }

    public void LoseBoy() // Can be called to dismiss follower.
    {
        hasBoy = false;
    }

    #endregion

    #region MOVEMENT

    // Movement amounts dictated by Vector2 moveVector value each fixedUpdate. Here's a list of public functions to be called where ever needed to change moveVector.

    public void SetMoveVector(Vector2 newMoveVector) // Function to completely override and replace existing moveVector.
    {
        moveVector = newMoveVector;
    }

    public void SetHorizontalMovement(float newHorizontalMovement) // Override only X value.
    {
        moveVector.x = newHorizontalMovement;
    }

    public void SetVerticalMovement(float newVerticalMovement) // Override only Y value.
    {
        moveVector.y = newVerticalMovement;
    }

    public void IncrementMovement(Vector2 additionalMovement) // Increment both X and Y by an amount, i.e. add additional movement to existing movement.
    {
        moveVector += additionalMovement;
    }

    public void IncrementHorizontalMovement(float additionalHorizontalMovement) // Increment only X.
    {
        moveVector.x += additionalHorizontalMovement;
    }

    public void IncrementVerticalMovement(float additionalVerticalMovement) // Increment only Y.
    {
        moveVector.y += additionalVerticalMovement;
    }

    public Vector2 GetMoveVector() // Retrieve current value of moveVector.
    {
        return moveVector;
    }

    public void GroundHorizontalMovement(bool useInput, float speedScale = 1f) // Function to be called during all on the ground state machine behaviours to allow for horizontal ground movement.
    {
        if ((PlayerInput.Instance.Horizontal.Value > 0 && moveVector.x < 0) || (PlayerInput.Instance.Horizontal.Value < 0 && moveVector.x > 0))
            moveVector.x = 0;

        float desiredSpeed = useInput ? PlayerInput.Instance.Horizontal.Value * maxSpeed * speedScale : 0f; // Retrieves desired speed based off if the value of the horizontal input value, allowing control sticks to be used for fine movements
        float acceleration = useInput && PlayerInput.Instance.Horizontal.ReceivingInput ? groundAcceleration : groundDeceleration; // Acceleration based off whether or not the horizontal axis is receiving input. Accell for receiving and decell for not.

        moveVector.x = Mathf.MoveTowards(moveVector.x, desiredSpeed, acceleration * Time.deltaTime); // Set the move vector's X value to be a value that increases to the max speed over time

        if (moveVector.x != 0) // If the player is moving in the x direction...
            PlayFootstep(); // Play footstep audio
    }

    public void GroundVerticalMovement() // Function called during on the ground SMB's to keep the player on the ground, especially during uneven ground
    {
        moveVector.y -= gravity * Time.deltaTime; // Decrement the moveVector's Y by gravity's acceleration * time

        if (moveVector.y < -gravity * Time.deltaTime * groundStickVelocityMultiplier) // If the current Y value of moveVector is less than gravity's descent * the multiplier...
        {
            moveVector.y = -gravity * Time.deltaTime * groundStickVelocityMultiplier; // Set it to be the gravity * multiplier because it is falling too quickly when it hits the ground.
        }
    }

    public bool CheckGrounded() // Function called in every SMB to determine if the player is on the ground or not
    {
        bool wasGrounded = animator.GetBool(hashGroundedParameter); // Check the current value of the grounded animator parameter
        bool grounded = characterController2D.Grounded; // Check to see if character is actually grounded
        
        if (grounded)
        {
            //FindCurrentSurface();

            if (!wasGrounded && moveVector.y < -1.0f) // If wasn't grounded before becoming grounded && being affected by gravity
            {
                landingAudioPlayer.PlayRandomSound(); // Play landing audio
                //landingAudioPlayer.PlayRandomSound(currentSurface);
            }

            if (wallSlideOn)
            {
                if (airborneDecelProportion != 0.5f) // If not normal level...
                    SetAirborneDecelProportion(0.5f); // Reset deceleration proportion to normal level. Wallslide reduces deceleration to allow for longer wall jumps

                if (!canSlide) // If wall slide is turned on, but is disabled from timing out...
                    canSlide = true; // Reenable wall slide once back on the ground
            }
            
        }
        else
        {
            currentSurface = null; // Reset current surface to be reassigned when landing
        }

        animator.SetBool(hashGroundedParameter, grounded); // Set animator paramter for grounded to character controller grounded

        return grounded; // Return true if on ground
    }

    public void FindCurrentSurface() // Function that finds the current surface the player is on. Used to play different walking/landing audio for different surfaces.
    {
        Collider2D groundCollider = characterController2D.GroundColliders[0]; 

        if (groundCollider == null)
        {
            groundCollider = characterController2D.GroundColliders[1];
        }

        if (groundCollider == null)
        {
            return;
        }

        TileBase b = PhysicsHelper.FindTileForOverride(groundCollider, transform.position, Vector2.down);
        if (b != null)
        {
            currentSurface = b;
        }
    }

    public void SetAirborneDecelProportion(float proportion, bool delay = false, float time = 0.02f) // Reduces airborne deceleration over time or instantly.
    {
        Mathf.Clamp(proportion, 0, 1);
        if (delay)
            StartCoroutine(SetProportion(proportion, time));
        else
            airborneDecelProportion = proportion;
    }

    IEnumerator SetProportion(float proportion, float time)
    {
        yield return new WaitForSeconds(time);
        airborneDecelProportion = proportion;
    }

    public void AirHorizontalMovement()
    {
        //if ((PlayerInput.Instance.Horizontal.Value > 0 && moveVector.x < 0) || (PlayerInput.Instance.Horizontal.Value < 0 && moveVector.x > 0))
        //    moveVector.x = 0;

        float desiredSpeed = PlayerInput.Instance.Horizontal.Value * maxSpeed; // Get speed from player input value * maxSpeed
        float acceleration;

        if (PlayerInput.Instance.Horizontal.ReceivingInput) // If the player is using horizontal input
        {
            acceleration = groundAcceleration * airborneAccelProportion; // Set acceleration
        }
        else
        {
            acceleration = groundDeceleration * airborneDecelProportion; // Set deceleration
        }
        
        moveVector.x = Mathf.MoveTowards(moveVector.x, desiredSpeed, acceleration * Time.deltaTime); // Increase current x speed to desired speed over time
    }

    public void AirVerticalMovement() // Set gravity
    {
        if (Mathf.Approximately(moveVector.y, 0f) || characterController2D.OnCeiling && moveVector.y > 0f) // If the y move vector is effectively 0 or the player hits the ceiling...
            moveVector.y = 0f; // Set y movement to 0.

        moveVector.y -= gravity * Time.deltaTime; // Subtract gravity from y move vector
    }

    public bool IsRising() // Check to see if the player is rising.
    {
        return moveVector.y > 0f;
    }

    public bool IsFalling() // Check to see if the player is falling and not on the ground.
    {
        return moveVector.y < 0f && !animator.GetBool(hashGroundedParameter);
    }

    public void PlayFootstep() // Plays footstep audio. Can set up to trigger vfx, as well.
    {
        footstepAudioPlayer.PlayRandomSound();
        //var footstepPosition = transform.position;
        //footstepPosition.z -= 1;
        //VFXController.Instance.Trigger("DustPuff", footstepPosition, 0, false, null, currentSurface);
    }

    #endregion

    #region FACING

    public void ForceFacing(bool faceLeft) // Force the direction the player is facing.
    {
        if (faceLeft)
        {
            spriteRenderer.flipX = !spriteOriginallyFacesLeft;
            currentBulletSpawnPoint = facingLeftBulletSpawnPoint;
        }
        else
        {
            spriteRenderer.flipX = spriteOriginallyFacesLeft;
            currentBulletSpawnPoint = facingRightBulletSpawnPoint;
        }
    }

    public void UpdateFacing() // Change the direction the player is facing based on directional movement.
    {
        bool faceLeft = PlayerInput.Instance.Horizontal.Value < 0f;
        bool faceRight = PlayerInput.Instance.Horizontal.Value > 0f;

        if (faceLeft)
        {
            spriteRenderer.flipX = !spriteOriginallyFacesLeft;
            currentBulletSpawnPoint = facingLeftBulletSpawnPoint;
        }
        else if (faceRight)
        {
            spriteRenderer.flipX = spriteOriginallyFacesLeft;
            currentBulletSpawnPoint = facingRightBulletSpawnPoint;
        }
    }

    public float GetFacing() // Returns the direction the player is facing. -1 = left, 1 = right.
    {
        return spriteRenderer.flipX != spriteOriginallyFacesLeft ? -1f : 1f;
    }

#endregion

    #region PUSHING

    // Hard coded pushing used instead of rigidbody pushing in order to set distance between player and pushable for proper animation spacing.
    // Since current art is pixel-based, the pushable can look a little shaky. Working on a fix for this.

    public void CheckForPushing() // State Machine Behaviour function. Checks if pushable is on the correct side, in the list of pushables.
    {
        bool pushableOnCorrectSide = false;
        Pushable previousPushable = currentPushable;

        currentPushable = null;

        if (currentPushables.Count > 0)
        {
            bool movingRight = PlayerInput.Instance.Horizontal.Value > float.Epsilon;
            bool movingLeft = PlayerInput.Instance.Horizontal.Value < -float.Epsilon;

            for (int i = 0; i < currentPushables.Count; i++)
            {
                float pushablePosX = currentPushables[i].pushablePosition.position.x; // Sets player position to set pushable position for proper animation spacing.
                float playerPosX = transform.position.x;
                if (pushablePosX < playerPosX && movingLeft || pushablePosX > playerPosX && movingRight)
                {
                    pushableOnCorrectSide = true;
                    currentPushable = currentPushables[i];
                    break;
                }
            }

            if (pushableOnCorrectSide)
            {
                Vector2 moveToPosition = movingRight ? currentPushable.playerPushingRightPosition.position : currentPushable.playerPushingLeftPosition.position;
                moveToPosition.y = characterController2D.Rigidbody.position.y;
                characterController2D.Teleport(moveToPosition);
            }
        }

        if (previousPushable != null && currentPushable != previousPushable)
            previousPushable.EndPushing();

        animator.SetBool(hashPushingPara, pushableOnCorrectSide); // Set animation state pushing parameter to switch to pushing State Machine Behaviour.
    }

    public void MovePushable()
    {
        if (currentPushable && currentPushable.Grounded) // If in list of pushables and on the ground
            currentPushable.Move(moveVector * Time.deltaTime); // Move pushable the same speed as the player.
    }

    public void StartPushing()
    {
        if (currentPushable)
            currentPushable.StartPushing();
    }

    public void StopPushing()
    {
        if (currentPushable)
            currentPushable.EndPushing();
    }

#endregion
    
    #region JUMP

    public bool CheckForJumpInput() // SMB function
    {
        return PlayerInput.Instance.Jump.Down;
    }

    public bool CheckForFallInput() // SMB function
    {
        return (PlayerInput.Instance.Vertical.Value < -float.Epsilon && PlayerInput.Instance.Jump.Down);
    }

    public void UpdateJump() // Updates jump to reduce height if jump key let go early.
    {
        if (!PlayerInput.Instance.Jump.Held && moveVector.y > 0.0f)
        {
            moveVector.y -= jumpAbortSpeedReduction * Time.deltaTime;
        }
    }

    public bool MakePlatformFallThrough() // Checks if the ground is the correct kind of collider, then uses FallThroughResetter to change platform's layer and back to allow player to fall through.
    {
        int colliderCount = 0;
        int fallThroughColliderCount = 0;

        for (int i = 0; i < characterController2D.GroundColliders.Length; i++)
        {
            Collider2D collider = characterController2D.GroundColliders[i];

            if (collider == null)
                continue;

            colliderCount++;

            if (PhysicsHelper.ColliderHasPlatformEffector(collider))
                fallThroughColliderCount++;
        }

        if (fallThroughColliderCount == colliderCount)
        {
            for (int i = 0; i < characterController2D.GroundColliders.Length; i++)
            {
                Collider2D collider = characterController2D.GroundColliders[i];
                if (collider == null)
                    continue;

                PlatformEffector2D effector;
                PhysicsHelper.TryGetPlatformEffector(collider, out effector);
                FallThroughResetter reseter = effector.gameObject.AddComponent<FallThroughResetter>();
                reseter.StartFall(effector);

                StartCoroutine(FallThroughInvincibility());
            }
        }

        return fallThroughColliderCount == colliderCount;
    }

    IEnumerator FallThroughInvincibility() // Prevents player from getting hurt when falling through platforms.
    {
        damageable.EnableInvulnerability(true);
        yield return new WaitForSeconds(0.5f);
        damageable.DisableInvulnerability();
    }

    #endregion

    #region WALLSLIDE

    public void WallSlideUpVerticalMovement() // Gravity applied to the player when wall sliding with upwards velocity.
    {
        moveVector.y -= wallSlideUpGravity * Time.deltaTime;
    }

    public void WallSlideDownVerticalMovement() // Gravity applied to the player when wall sliding with downwards velocity.
    {
        moveVector.y = 0f;
        moveVector.y -= (wallSlideDownGravity * Time.deltaTime);
    }

    public bool TouchingWall() // check if the characterController is touching the wall.
    {
        return characterController2D.TouchingWall;
    }

    public void CheckWallSlide() // Set's wall slide state bools to switch into and out of the wall slide state.
    {
        if (canSlide)
        {
            if (characterController2D.TouchingWall)
            {
                animator.SetBool(hashWallSlideParameter, true);
            }
            else
            {
                animator.SetBool(hashWallSlideParameter, false);
            }
        }
    }

    public void SetWallSlideTimeout() // Sets time to original value.
    {
        wallSlideTimeRemaining = wallSlideTimeoutDuration;
    }

    public void WallSlideTimeout() // Runs timer while wall sliding. Stops wall slide when timer runs out.
    {
        if (wallSlideTimeRemaining <= 0.0f)
        {
            ForceNotWallSlide();
            DisableWallSlide();
            return;
        }

        wallSlideTimeRemaining -= Time.deltaTime; // Subtracts the change in time each frame from the time remaining to create an accurate timer.
    }

    public void ForceNotWallSlide() // Function to force exit of wall slide state
    {
        animator.SetBool(hashWallSlideParameter, false);
    }

    public void EnableWallSlide() // Enables the wall slide ability
    {
        canSlide = true;
    }

    public void DisableWallSlide() // Disables the wall slide ability
    {
        canSlide = false;
    }

    #endregion

    #region MELEE ATTACK

    public bool CheckForMeleeAttackInput() // SMB function
    {
        return PlayerInput.Instance.MeleeAttack.Down;
    }

    public void MeleeAttack() // SMB trigger
    {
        animator.SetTrigger(hashMeleeAttackParameter);
    }

    public void EnableMeleeDamager() // Triggers damage coroutine when melee attack initiated.
    {
        StartCoroutine(Damage());
    }

    IEnumerator Damage() // Triggers damager for melee attack
    {
        yield return new WaitForSeconds(meleeDamagerDelay);
        meleeDamager.EnableDamage();
        meleeDamager.disableDamageAfterHit = true;
    }

    public void DisableMeleeDamager() // Force disables damager
    {
        meleeDamager.DisableDamage();
    }

    public void EnableMeleeAttack() // Enables the melee attack ability
    {
        PlayerInput.Instance.EnableMeleeAttacking();
    }

    public void DisableMeleeAttack() // Disables the melee attack ability
    {
        PlayerInput.Instance.DisableMeleeAttacking();
    }

    public void TeleportToColliderBottom() // Unrelated to melee attack. Teleports player to the bottom of their collider to reset their position.
    {
        Vector2 colliderBottom = characterController2D.Rigidbody.position + collider.offset + Vector2.down * collider.size.y * 0.5f;
        characterController2D.Teleport(colliderBottom);
    }

    #endregion

    #region RANGED ATTACK

    public void SetBulletCount(int count) // Sets how many bullets the player can fire at once.
    {
        bulletPool.initialPoolCount = count;
    }

    protected void UpdateBulletSpawnPointPositions() // Update's opposite side's bullet spawn point if one of them is animated to move with animation. Currently they are not animated.
    {
        if (rightBulletSpawnPointAnimated)
        {
            Vector2 leftPosition = facingRightBulletSpawnPoint.localPosition;
            leftPosition.x *= -1f;
            facingLeftBulletSpawnPoint.localPosition = leftPosition;
        }
        else
        {
            Vector2 rightPosition = facingLeftBulletSpawnPoint.localPosition;
            rightPosition.x *= -1f;
            facingRightBulletSpawnPoint.localPosition = rightPosition;
        }
    }

    protected IEnumerator Shoot() // Spawns bullets at regular intervals while ranged button held.
    {
        while (PlayerInput.Instance.RangedAttack.Held)
        {
            if (Time.time >= nextShotTime)
            {
                SpawnBullet();
                nextShotTime = Time.time + shotSpawnGap;
            }
            yield return null;
        }
    }

    protected void SpawnBullet() // Tests location of bullet spawn to make sure it isn't on the other side of a wall. If ok, spawns bullet by popping from bullet pool. Adds velocity in proper direction.
    {
        Vector2 testPosition = transform.position;
        testPosition.y = currentBulletSpawnPoint.position.y;
        Vector2 direction = (Vector2)currentBulletSpawnPoint.position - testPosition;
        float distance = direction.magnitude;
        direction.Normalize();

        RaycastHit2D[] results = new RaycastHit2D[12];
        if (Physics2D.Raycast(testPosition, direction, characterController2D.GroundContactFilter, results, distance) > 0)
        {
            return;
        }

        BulletObject bullet = bulletPool.Pop(currentBulletSpawnPoint.position);
        bool facingLeft = currentBulletSpawnPoint == facingLeftBulletSpawnPoint;
        bullet.rigidbody2D.velocity = new Vector2(facingLeft ? -bulletSpeed : bulletSpeed, 0f);
        bullet.spriteRenderer.flipX = facingLeft ^ bullet.bullet.spriteOriginallyFacesLeft;

        rangedAttackAudioPlayer.PlayRandomSound();
    }

    public bool CheckForRangedAttackOut() // Check that keeps track of time ranged attack is out. After set time, player will put gun away (exit ranged SMB).
    {
        bool rangedAttack = false;

        if (PlayerInput.Instance.RangedAttack.Held)
        {
            rangedAttack = true;
            animator.SetBool(hashRangedAttackParameter, true);
            rangedAttackTimeRemaining = holdingGunTimeoutDuration;
        }
        else
        {
            rangedAttackTimeRemaining -= Time.deltaTime;

            if (rangedAttackTimeRemaining <= 0f)
            {
                animator.SetBool(hashRangedAttackParameter, false);
            }
        }

        return rangedAttack;
    }

    public void CheckAndFireGun() // Starts Shoot coroutine while ranged attack button held and stops when ranged attack button let go
    {
        if (PlayerInput.Instance.RangedAttack.Held && animator.GetBool(hashRangedAttackParameter))
        {
            if (shootingCoroutine == null)
            {
                shootingCoroutine = StartCoroutine(Shoot());
            }
        }

        if ((PlayerInput.Instance.RangedAttack.Up || !animator.GetBool(hashRangedAttackParameter)) && shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    public void ForceNotRangedAttack() // Forces player to exit ranged SMB.
    {
        animator.SetBool(hashRangedAttackParameter, false);
    }

    public void EnableRangedAttack() // Enables ranged attack ability.
    {
        PlayerInput.Instance.EnableRangedAttacking();
    }

    public void DisableRangedAttack() // Disables ranged attack ability.
    {
        PlayerInput.Instance.DisableRangedAttacking();
    }

    #endregion

    #region INVULNERABILITY

    public void EnableInvulnerability() // Enables damageable's invulnerability. Can be called when picking up an item e.g. a star in Mario.
    {
        damageable.EnableInvulnerability();
    }

    public void DisableInvulnerability() // Disables damageable's invulnerability.
    {
        damageable.DisableInvulnerability();
    }

    #endregion

    #region FLICKER

    protected IEnumerator Flicker() // Flickers player's sprite renderer when damaged/invulnerable.
    {
        float timer = 0f; // Set timer variable.

        while (timer < damageable.invulnerabilityDuration) // While timer hasn't reached set time limit...
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // If sprite renderer is enabled, disable it. If sprite renderer is disabled, enable it.
            yield return flickeringWait; // Wait set time between flickers.
            timer += flickeringDuration; // Add set time increment to timer.
        }

        spriteRenderer.enabled = true; // Ensure sprite renderer is enabled when finished.
    }

    public void StartFlickering()
    {
        flickerCoroutine = StartCoroutine(Flicker());
    }

    public void StopFlickering()
    {
        StopCoroutine(flickerCoroutine);
        spriteRenderer.enabled = true;
    }

    #endregion

    #region TAKE DAMAGE

    public Vector2 GetHurtDirection() // Determines which side the player got hit on. Finds vector2 for hurt direction based on direction and set hurt jump angle.
    {
        Vector2 damageDirection = damageable.GetDamageDirection();

        if (damageDirection.y < 0f)
        {
            return new Vector2(Mathf.Sign(damageDirection.x), 0f);
        }

        float y = Mathf.Abs(damageDirection.x) * tanHurtJumpAngle;

        return new Vector2(damageDirection.x, y).normalized;
    }

    public void OnHurt(Damager damager, Damageable damageable) // Called when player gets hurt. Calculates damage, triggers invulnerability, respawns if needed.
    {
        if (!PlayerInput.Instance.HaveControl)
        {
            return;
        }

        ForceFacing(damageable.GetDamageDirection().x > 0f); // Force player to face direction of damage, so they are knocked "back".
        damageable.EnableInvulnerability();

        animator.SetTrigger(hashHurtParameter);

        if (damageable.CurrentHealth > 0 && damager.forceRespawn)
        {
            animator.SetTrigger(hashForcedRespawnParameter);
        }

        animator.SetBool(hashGroundedParameter, false);
        hurtAudioPlayer.PlayRandomSound();

        if (damager.forceRespawn && damageable.CurrentHealth > 0)
        {
            StartCoroutine(DieRespawnCoroutine(false, true, 1.0f));
        }
    }

    #endregion

    #region DIE

    public void OnDie() // Forces death. Not being used.
    {
        animator.SetTrigger(hashDeadParameter);

        StartCoroutine(DieRespawnCoroutine(true, false, 1.0f));
    }

    public void TriggerDieRespawn(bool resetHealth, bool useCheckPoint, float waitTime) // Force triggers respawn. Used by FallRespawn.
    {
        StartCoroutine(DieRespawnCoroutine(resetHealth, useCheckPoint, waitTime));
    }

    IEnumerator DieRespawnCoroutine(bool resetHealth, bool useCheckPoint, float waitTime) // Release user control. Fade out. Respawn. Fade in. Regain control. Currently set up to respawn player on a "gameover" death. Can be set up to pause of gameover canvas and offer choices.
    {
        PlayerInput.Instance.ReleaseControl(true);
        yield return new WaitForSeconds(waitTime);

        yield return StartCoroutine(ScreenFader.FadeSceneOut(useCheckPoint ? ScreenFader.FadeType.Black : ScreenFader.FadeType.GameOver));

        if (!useCheckPoint)
            yield return new WaitForSeconds(2.0f);

        Respawn(resetHealth, useCheckPoint); // Triggers actual respawn.

        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(ScreenFader.FadeSceneIn());
        PlayerInput.Instance.GainControl();
    }

    #endregion

    #region RESPAWN

    public void Respawn(bool resetHealth, bool useCheckpoint) // Respawns character. Can use level start or checkpoint and can reset health if needed.
    {
        if (resetHealth)
        {
            damageable.SetHealth(damageable.startingHealth);
        }
        
        animator.ResetTrigger(hashHurtParameter); // Stops being "hurt" since respawning
        if (flickerCoroutine != null)
        {
            StopFlickering();
        }
        animator.SetTrigger(hashRespawnParameter);

        if (useCheckpoint && lastCheckpoint != null)
        {
            ForceFacing(lastCheckpoint.respawnFacingLeft);
            GameObjectTeleporter.Teleport(gameObject, lastCheckpoint.transform.position); // Teleports player to last checkpoint
            CheckGrounded();
            if (boy != null)
                boy.Respawn(lastCheckpoint.transform.position);
        }
        else
        {
            ForceFacing(startingFacingLeft);
            GameObjectTeleporter.Teleport(gameObject, startingPosition); // Teleports player to level start.
            CheckGrounded();
            animator.SetBool(hashGroundedParameter, true);
            if (boy != null)
                boy.Respawn(startingPosition);
        }
    }

    public void SetCheckpoint(Checkpoint checkpoint) // Called by checkpoints to set most recent checkpoint location. 
    {
        lastCheckpoint = checkpoint;
    }

    #endregion
}
