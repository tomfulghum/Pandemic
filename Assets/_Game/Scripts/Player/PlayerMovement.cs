using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Actor2D))]

public class PlayerMovement : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [Header("Movement")]
    [Tooltip("Horizontal movement speed (u/s).")]
    [SerializeField] float movementSpeed = 7f;
    [Tooltip("Time it takes for the player to reach maximum movement speed while grounded (s).")]
    [SerializeField] float groundAccelerationTime = 0.1f;
    [Tooltip("Time it takes for the player to reach maximum movement speed while in the air (s).")]
    [SerializeField] float airAccelerationTime = 0.3f;
    [Tooltip("Maximum jump height (u).")]
    [SerializeField] float maxJumpHeight = 4f;
    [Tooltip("Vertical upward speed while jumping (u/s).")]
    [SerializeField] float jumpSpeed = 10f;
    [Tooltip("Factor by which the upward speed is multiplied when canceling a jump.")]
    [Range(0f, 1f)]
    [SerializeField] float jumpCancelSpeedMultiplier = 0.25f;
    [Tooltip("Downward acceleration (u/s²).")]
    [SerializeField] float gravity = 30f;
    [Tooltip("Maximum vertical downward speed while falling (u/s).")]
    [SerializeField] float maxFallingSpeed = 20f;
    [Tooltip("Time after losing ground during which jumping is still possible (s).")]
    [SerializeField] float groundToleranceTime = 0.05f;

    //**********************//
    //    Private Fields    //
    //**********************//

    private PlayerInput input;
    private Actor2D actor;

    private Vector2 moveDirection = Vector2.zero;
    private Vector2 lastMoveDirection = Vector2.zero;
    private Vector2 externalVelocity = Vector2.zero;

    private Coroutine jumpCoroutine = null;
    private CollisionData lastCollision;

    private bool groundTolerance = false;
    private bool jumping = false;
    private bool jumpCanceled = false;
    private bool inputDisabled = false;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        actor = GetComponent<Actor2D>();
    }

    private void Update()
    {
        // Movement input
        float inputX = input.player.GetAxisRaw(input.moveHorizontalAxis);
        moveDirection = new Vector2(inputX, 0).normalized;

        // Jump
        if ((actor.collision.below || groundTolerance) && !actor.collision.above && !inputDisabled && input.player.GetButtonDown(input.jumpButton)) {
            jumpCoroutine = StartCoroutine(JumpCoroutine());
        }
        if (jumping && (input.player.GetButtonUp(input.jumpButton) || actor.collision.above)) {
            jumpCanceled = true;
        }

        HandleCollisions();

        // Apply movement data
        if (!inputDisabled) {
            Move(CalculateVelocity());

            if (!jumping) {
                ApplyGravity();
            }

            // Clamp velocity to maximum vertical movement speed.
            actor.velocity = new Vector2(actor.velocity.x, Mathf.Clamp(actor.velocity.y, -maxFallingSpeed, float.MaxValue));
        } else {
            actor.velocity = externalVelocity;
        }

        lastCollision = actor.collision;
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    // Calculates the current normalized horizontal velocity based on input direction and acceleration times.
    Vector2 CalculateVelocity()
    {
        float directionChangeModifier = Util.SameSign(moveDirection.x, lastMoveDirection.x) ? 1f : 0f;
        float accelerationTime = actor.collision.below ? groundAccelerationTime : airAccelerationTime;
        accelerationTime = accelerationTime > 0 ? accelerationTime : 0.001f;

        Vector2 velocity = Vector2.MoveTowards(lastMoveDirection * directionChangeModifier, moveDirection, 1f / accelerationTime * Time.deltaTime);
        lastMoveDirection = velocity;
        return velocity;
    }

    // Moves the character in a specified direction.
    private void Move(Vector2 _direction)
    {
        actor.velocity = new Vector2(_direction.x * movementSpeed, actor.velocity.y);
    }

    // Applies gravity to the character.
    private void ApplyGravity()
    {
        actor.velocity += Vector2.up * (-gravity * Time.deltaTime);
    }

    // Handles collisions and sets velocity accordingly
    private void HandleCollisions()
    {
        // Ground tolerance
        if (!actor.collision.below && lastCollision.below && !jumping) {
            StartCoroutine(GroundToleranceCoroutine());
        }

        if (actor.collision.below && actor.collision.below.CompareTag("MovingObject")) {
            actor.master = actor.collision.below.GetComponent<MovingObject>();
        } else if (!actor.collision.below || (actor.collision.below && !actor.collision.below.CompareTag("MovingObject"))) {
            actor.master = null;
        }

        // Collisions
        if (actor.collision.above || actor.collision.below) {
            actor.velocity = new Vector2(actor.velocity.x, 0);
        }
        if (actor.collision.left || actor.collision.right) {
            actor.velocity = new Vector2(0, actor.velocity.y);
        }
    }

    // Cancels an ongoing jump.
    private void CancelJump()
    {
        Vector2 velocity = actor.velocity;
        actor.velocity = new Vector2(velocity.x, velocity.y * jumpCancelSpeedMultiplier);
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
        float accelerationEndTime = Time.time + accelerationTime;
        float jumpEndTime = Time.time + accelerationTime + floatingTime;

        while (Time.time <= jumpEndTime) {
            if (jumpCanceled || actor.collision.above) {
                CancelJump();
                yield break;
            }

            if (Time.time <= accelerationEndTime) {
                actor.velocity = new Vector2(actor.velocity.x, jumpSpeed);
            } else {
                ApplyGravity();
            }

            yield return null;
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

    //************************//
    //    Public Functions    //
    //************************//

    // _freeze = true: Cancels all movement and prevents all input.
    // _freeze = false: Sets external velocity to zero and enables all input.
    public void DisableUserInput(bool _disable)
    {
        if (inputDisabled == _disable) {
            return;
        }

        inputDisabled = _disable;
        if (inputDisabled) {
            if (jumpCoroutine != null) {
                StopCoroutine(jumpCoroutine);
                CancelJump();
            }
            externalVelocity = Vector2.zero;
            actor.velocity = Vector2.zero;
        } else {
            externalVelocity = Vector2.zero;
        }
    }

    // Sets the external velocity. Only has an effect if freeze is true.
    public void SetExternalVelocity(Vector2 _velocity)
    {
        externalVelocity = _velocity;
    }
}