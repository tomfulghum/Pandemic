using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerMovement : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [Header("Movement")]
    [Tooltip("Horizontal movement speed (u/s).")]
    [SerializeField] float movementSpeed = 6f;
    [Tooltip("Time it takes for the player to reach maximum movement speed while grounded (s).")]
    [SerializeField] float groundAccelerationTime = 0.1f;
    [Tooltip("Time it takes for the player to reach maximum movement speed while in the air (s).")]
    [SerializeField] float airAccelerationTime = 0.3f;
    [Tooltip("Maximum jump height (u).")]
    [SerializeField] float maxJumpHeight = 3f;
    [Tooltip("Vertical upward speed while jumping (u/s).")]
    [SerializeField] float jumpSpeed = 6f;
    [Tooltip("Factor by which the upward speed is multiplied when canceling a jump.")]
    [Range(0f, 1f)]
    [SerializeField] float jumpCancelSpeedMultiplier = 0.25f;
    [Tooltip("Downward acceleration (u/s²).")]
    [SerializeField] float gravity = 30f;
    [Tooltip("Maximum vertical downward speed while falling (u/s).")]
    [SerializeField] float maxFallingSpeed = 10f;
    [Tooltip("Time after losing ground during which jumping is still possible (s).")]
    [SerializeField] float groundToleranceTime = 0.05f;

    [Header("Vertical collision checks")]
    [SerializeField] LayerMask collisionCheckLayers = default;
    [SerializeField] Vector2 groundCheckOffset = Vector2.zero;
    [SerializeField] Vector2 groundCheckSize = Vector2.one;
    [SerializeField] Vector2 ceilingCheckOffset = Vector2.zero;
    [SerializeField] Vector2 ceilingCheckSize = Vector2.one;

    //******************//
    //    Properties    //
    //******************//

    //**********************//
    //    Private Fields    //
    //**********************//

    private Rigidbody2D rb;

    private Vector2 moveDirection = Vector2.zero;
    private Vector2 lastMoveDirection = Vector2.zero;
    private Vector2 externalVelocity = Vector2.zero;

    private bool ground = false;
    private bool ceiling = false;
    private bool groundTolerance = false;
    private bool jumping = false;
    private bool jumpCanceled = false;
    private bool inputDisabled = false;

    private Coroutine jumpCoroutine = null;

    //-----------------------//
    //    Unity Functions    //
    //-----------------------//

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        moveDirection = new Vector2(x, 0).normalized;

        if ((ground || groundTolerance) && !ceiling && !inputDisabled && Input.GetButtonDown("Jump") ) {
            jumpCoroutine = StartCoroutine(JumpCoroutine());
        }
        if (jumping && (Input.GetButtonUp("Jump") || ceiling)) {
            jumpCanceled = true;
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        if (!inputDisabled) {
            Move(CalculateVelocity());

            if (!jumping) {
                ApplyGravity();
            }

            // Clamp velocity to maximum horizontal and vertical movement speeds.
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -movementSpeed, movementSpeed), Mathf.Clamp(rb.velocity.y, -maxFallingSpeed, jumpSpeed));
        } else {
            rb.velocity = externalVelocity;
        }
    }

    //-------------------------//
    //    Private Functions    //
    //-------------------------//

    // Checks if the character is touching the ground/ceiling and sets the ground/ceiling flags accordingly.
    private void CheckCollisions()
    {
        Vector2 position = transform.position;
        Collider2D groundHit = Physics2D.OverlapBox(position + groundCheckOffset, groundCheckSize, 0, collisionCheckLayers);
        Collider2D ceilingHit = Physics2D.OverlapBox(position + ceilingCheckOffset, ceilingCheckSize, 0, collisionCheckLayers);

        bool newGround = groundHit;
        if (ground && !newGround && !jumping) {
            StartCoroutine(GroundToleranceCoroutine());
        }

        ground = newGround;
        ceiling = ceilingHit;
    }

    // Calculates the current normalized horizontal velocity based on input direction and acceleration times.
    Vector2 CalculateVelocity()
    {
        float directionChangeModifier = Util.SameSign(moveDirection.x, lastMoveDirection.x) ? 1f : 0f;
        float accelerationTime = ground ? groundAccelerationTime : airAccelerationTime;
        accelerationTime = accelerationTime > 0 ? accelerationTime : 0.001f;

        Vector2 velocity = Vector2.MoveTowards(lastMoveDirection * directionChangeModifier, moveDirection, 1f / accelerationTime * Time.deltaTime);
        lastMoveDirection = velocity;
        return velocity;
    }

    // Moves the character in a specified direction.
    private void Move(Vector2 _direction)
    {
        rb.velocity = new Vector2(_direction.x * movementSpeed, rb.velocity.y);
    }

    // Applies gravity to the character.
    private void ApplyGravity()
    {
        rb.velocity += Vector2.up * (-gravity * Time.fixedDeltaTime);
    }
    
    // Cancels an ongoing jump.
    private void CancelJump()
    {
        Vector2 velocity = rb.velocity;
        rb.velocity = new Vector2(velocity.x, velocity.y * jumpCancelSpeedMultiplier);
        jumpCanceled = false;
        jumping = false;
        jumpCoroutine = null;
    }

    // The coroutine that controls a jump.
    private IEnumerator JumpCoroutine()
    {
        jumping = true;

        // Times and distances used to calculate how long to apply acceleration.
        float floatingDistance = (jumpSpeed * jumpSpeed) / (2f * gravity);
        float floatingTime = jumpSpeed / gravity;
        float accelerationTime = (maxJumpHeight - floatingDistance) / jumpSpeed;
        float accelerationEndTime = Time.fixedTime + accelerationTime;
        float jumpEndTime = Time.fixedTime + accelerationTime + floatingTime;

        while (Time.fixedTime <= jumpEndTime) {
            if (jumpCanceled || ceiling) {
                CancelJump();
                yield break;
            }

            if (Time.fixedTime <= accelerationEndTime) {
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            } else {
                ApplyGravity();
            }

            yield return new WaitForFixedUpdate();
        }

        jumping = false;
        jumpCoroutine = null;
    }

    // Enables ground tolerance and disables it after a time.
    private IEnumerator GroundToleranceCoroutine()
    {
        groundTolerance = true;
        yield return new WaitForSeconds(groundToleranceTime);
        groundTolerance = false;
    }

    //------------------------//
    //    Public Functions    //
    //------------------------//
    
    // _freeze = true: Cancels all movement and prevents all input.
    // _freeze = false: Sets external velocity to zero and enables all input.
    public void DisableUserInput(bool _disable)
    {
        inputDisabled = _disable;
        if (inputDisabled) {
            if (jumpCoroutine != null) {
                CancelJump();
                StopCoroutine(jumpCoroutine);
            }
            externalVelocity = Vector2.zero;
            rb.velocity = Vector2.zero;
        } else {
            externalVelocity = Vector2.zero;
        }
    }

    // Sets the external velocity. Only has an effect if freeze is true.
    public void SetExternalVelocity(Vector2 _velocity)
    {
        externalVelocity = _velocity;
    }

    //-----------------------//
    //    Debug Functions    //
    //-----------------------//

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 position = transform.position;
        Gizmos.DrawWireCube(position + (Vector3)groundCheckOffset, groundCheckSize);
        Gizmos.DrawWireCube(position + (Vector3)ceilingCheckOffset, ceilingCheckSize);
    }
}