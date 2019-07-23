using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]

public class PlayerMovement : MonoBehaviour
{
    [System.Serializable]
    private struct InputState
    {
        public Vector2 movement;
        public Vector2 lastMovement;
        public bool jump;
        public bool cancelJump;
        public float jumpTimer;
        public float jumpCancelTimer;
    }

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

    private PlayerInput m_input;
    private Actor2D m_actor;
    private Rigidbody2D m_rb;

    private Vector2 moveDirection = Vector2.zero;
    private Vector2 lastMoveDirection = Vector2.zero;
    private Vector2 m_externalVelocity = Vector2.zero;

    private Coroutine jumpCoroutine = null;
    private ContactData lastCollision;

    private bool groundTolerance = false;
    private bool jumping = false;
    private bool jumpCanceled = false;
    private bool m_inputDisabled = false;

    private InputState m_inputState = default;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_input = GetComponent<PlayerInput>();
        m_actor = GetComponent<Actor2D>();
        m_rb = GetComponent<Rigidbody2D>();

        m_inputState = new InputState();
    }

    private void FixedUpdate()
    {
        UpdateMovement();
        UpdateJump();
    }

    private void Update()
    {
        ProcessMovementInput();
        ProcessJumpInput();

        /*
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
        */
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void ProcessMovementInput()
    {
        float inputHorizontal = m_input.player.GetAxisRaw(m_input.moveHorizontalAxis);
        m_inputState.movement = new Vector2(inputHorizontal, 0);
    }

    private void ProcessJumpInput()
    {
        if (m_actor.contacts.below && m_input.player.GetButtonDown(m_input.jumpButton)) {
            m_inputState.jump = true;

            float absGravity = Mathf.Abs(Physics2D.gravity.y);
            float trueJumpSpeed = jumpSpeed - (absGravity * Time.fixedDeltaTime);
            float floatingDistance = (trueJumpSpeed * trueJumpSpeed) / (2f * absGravity);
            float floatingTime = trueJumpSpeed / absGravity;
            float accelerationTime = (maxJumpHeight - floatingDistance) / trueJumpSpeed;

            m_inputState.jumpTimer = accelerationTime;
            m_inputState.jumpCancelTimer = accelerationTime + floatingTime;
        }

        if (m_inputState.jump && m_input.player.GetButtonUp(m_input.jumpButton)) {
            m_inputState.cancelJump = true;
        }
    }

    private void UpdateMovement()
    {
        if (m_inputDisabled) {
            m_rb.velocity = m_externalVelocity;
            return;
        }

        float directionChangeModifier = Util.SameSign(m_inputState.movement.x, m_inputState.lastMovement.x) ? 1f : 0f;
        float accelerationTime = m_actor.contacts.below ? groundAccelerationTime : airAccelerationTime;
        Vector2 movement = m_inputState.movement;
        if (accelerationTime > 0) {
            movement = Vector2.MoveTowards(m_inputState.lastMovement * directionChangeModifier, m_inputState.movement, 1f / accelerationTime * Time.fixedDeltaTime);
        }

        m_rb.velocity = new Vector2(movement.x * movementSpeed, m_rb.velocity.y);
        m_inputState.lastMovement = movement;
    }

    private void UpdateJump()
    {
        if (m_actor.contacts.above) {
            m_inputState.jump = false;
            m_inputState.cancelJump = false;
            return;
        }

        if (m_inputState.cancelJump) {
            m_inputState.jump = false;
            m_inputState.cancelJump = false;
            m_rb.velocity = new Vector2(m_rb.velocity.x, m_rb.velocity.y * jumpCancelSpeedMultiplier);
            return;
        }

        if (m_inputState.jump) {
            if (m_inputState.jumpCancelTimer >= 0) {
                if (m_inputState.jumpTimer >= 0) {
                    m_rb.velocity = new Vector2(m_rb.velocity.x, jumpSpeed);
                    m_inputState.jumpTimer -= Time.fixedDeltaTime;
                }
                m_inputState.jumpCancelTimer -= Time.fixedDeltaTime;
            } else {
                m_inputState.jump = false;
            }
        }
    }

    /*
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

        while (Time.time < jumpEndTime) {
            if (jumpCanceled || actor.collision.above) {
                CancelJump();
                yield break;
            }

            if (Time.time < accelerationEndTime) {
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
    */

    //************************//
    //    Public Functions    //
    //************************//

    
    // _freeze = true: Cancels all movement and prevents all input.
    // _freeze = false: Sets external velocity to zero and enables all input.
    public void DisableUserInput(bool _disable)
    {
        if (m_inputDisabled == _disable) {
            return;
        }
        
        m_inputDisabled = _disable;
        m_inputState.lastMovement = Vector2.zero;
        m_externalVelocity = Vector2.zero;
    }

    // Sets the external velocity. Only has an effect if freeze is true.
    public void SetExternalVelocity(Vector2 _velocity)
    {
        m_externalVelocity = _velocity;
    }
}