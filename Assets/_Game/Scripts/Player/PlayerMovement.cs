using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Actor2D))]
[RequireComponent(typeof(PlayerInput))]

public class PlayerMovement : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    [System.Serializable]
    private struct InputState
    {
        public Vector2 movement;
        public Vector2 lastMovement;
        public bool jump;
        public bool cancelJump;
        public float jumpTimer;
        public float jumpCancelTimer;
        public float groundToleranceTimer;

        public void Reset()
        {
            movement = Vector2.zero;
            lastMovement = Vector2.zero;
            jump = false;
            cancelJump = false;
            jumpTimer = 0;
            jumpCancelTimer = 0;
            groundToleranceTimer = 0;
        }
    }

    //**********************//
    //   Inspector Fields   //
    //**********************//

    [Tooltip("Horizontal movement speed (u/s).")]
    [SerializeField] private float m_movementSpeed = 7f;
    [Tooltip("Time it takes for the player to reach maximum movement speed while grounded (s).")]
    [SerializeField] private float m_groundAccelerationTime = 0.1f;
    [Tooltip("Time it takes for the player to reach maximum movement speed while in the air (s).")]
    [SerializeField] private float m_airAccelerationTime = 0.3f;
    [Tooltip("Maximum jump height (u).")]
    [SerializeField] private float m_maxJumpHeight = 4f;
    [Tooltip("Vertical upward speed while jumping (u/s).")]
    [SerializeField] private float m_jumpSpeed = 10f;
    [Tooltip("Factor by which the upward speed is multiplied when canceling a jump.")]
    [Range(0f, 1f)]
    [SerializeField] private float m_jumpCancelSpeedMultiplier = 0.25f;
    [Tooltip("Maximum vertical downward speed while falling (u/s).")]
    [SerializeField] private float m_maxFallingSpeed = 20f;
    [Tooltip("Time after losing ground during which jumping is still possible (s).")]
    [SerializeField] private float m_groundToleranceTime = 0.05f;
    [Tooltip("The deceleration applied to momentum when not on a moving object (u/s²).")]
    [SerializeField] private float m_momentumDeceleration = 1.0f;

    //******************//
    //    Properties    //
    //******************//

    public Vector2 externalVelocity
    {
        get { return m_externalVelocity; }
        set { m_externalVelocity = value; }
    }

    public Vector2 momentum
    {
        get { return m_momentum; }
        set { m_momentum = value; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private PlayerInput m_input;
    private Actor2D m_actor;
    private Rigidbody2D m_rb;

    private Vector2 m_externalVelocity = Vector2.zero;
    public Vector2 m_momentum = Vector2.zero;
    private bool m_inputDisabled = false;

    private InputState m_inputState = default;
    private ContactData m_lastContacts;

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

        m_lastContacts = m_actor.contacts;
    }

    private void Update()
    {
        ProcessMovementInput();
        ProcessJumpInput();
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    // Processes the movement-related input and sets the input state accordingly.
    private void ProcessMovementInput()
    {
        float inputHorizontal = m_input.player.GetAxisRaw(m_input.moveHorizontalAxis);
        m_inputState.movement = new Vector2(inputHorizontal, 0);
    }

    // Processes all jump-related input and sets the input state accordingly.
    private void ProcessJumpInput()
    {
        // Calculate jump parameters if the player presses the jump button
        if (!m_inputDisabled && (m_actor.contacts.below || m_inputState.groundToleranceTimer >= 0) && m_input.player.GetButtonDown(m_input.jumpButton)) {
            m_inputState.jump = true;

            float absGravity = Mathf.Abs(Physics2D.gravity.y);
            float trueJumpSpeed = m_jumpSpeed - (absGravity * Time.fixedDeltaTime);
            float floatingDistance = (trueJumpSpeed * trueJumpSpeed) / (2f * absGravity);
            float floatingTime = trueJumpSpeed / absGravity;
            float accelerationTime = (m_maxJumpHeight - floatingDistance) / trueJumpSpeed;

            m_inputState.jumpTimer = accelerationTime;
            m_inputState.jumpCancelTimer = accelerationTime + floatingTime;

            GetComponent<PlayerAnim>().TriggerJumpAnim();
        }

        // Cancel jump if the player releases the jump button
        if (m_inputState.jump && m_input.player.GetButtonUp(m_input.jumpButton)) {
            m_inputState.cancelJump = true;
        }
    }

    // Updates the Rigidbody2D velocity according to movement input.
    private void UpdateMovement()
    {
        // Apply external velocity and return if input is disabled
        if (m_inputDisabled) {
            m_rb.velocity = m_externalVelocity;
            return;
        }

        Vector2 movement = m_inputState.movement;

        // Calculate movement acceleration
        float directionChangeModifier = Util.SameSign(m_inputState.movement.x, m_inputState.lastMovement.x) ? 1 : 0;
        float accelerationTime = m_actor.contacts.below ? m_groundAccelerationTime : m_airAccelerationTime;
        if (accelerationTime > 0) {
            movement = Vector2.MoveTowards(m_inputState.lastMovement * directionChangeModifier, m_inputState.movement, 1f / accelerationTime * Time.fixedDeltaTime);
        }

        // Calculate momentum
        float horizontalVelocity = 0;

        if (m_actor.master) {
            momentum = m_actor.master.velocity;
            horizontalVelocity = movement.x * m_movementSpeed + m_momentum.x;
        } else {
            m_momentum = Vector2.MoveTowards(m_momentum, Vector2.zero, m_momentumDeceleration * Time.fixedDeltaTime);

            if (!m_actor.contacts.below && movement.x != 0) {
                if (!Util.SameSign(m_momentum.x, movement.x)) {
                    m_momentum.x = 0;
                }
            }
            if (m_actor.contacts.below || m_actor.contacts.left || m_actor.contacts.right) {
                m_momentum.x = 0;
            }
            if (m_actor.contacts.below || m_actor.contacts.above) {
                m_momentum.y = 0;
            }

            horizontalVelocity = Mathf.Clamp(movement.x * m_movementSpeed + m_momentum.x, -m_movementSpeed, m_movementSpeed);
        }

        // Update velocity
        m_rb.velocity = new Vector2(horizontalVelocity, m_rb.velocity.y);

        // Clamp vertical speed to maximum falling speed
        m_rb.velocity = new Vector2(m_rb.velocity.x, Mathf.Clamp(m_rb.velocity.y, -m_maxFallingSpeed, float.MaxValue));

        m_inputState.lastMovement = movement;
    }

    // Updates the Rigidbody2D velocity as well as input state timers according to jump input.
    private void UpdateJump()
    {
        // Start ground tolerance timer if the player just lost ground
        if (!m_actor.contacts.below && m_lastContacts.below) {
            m_inputState.groundToleranceTimer = m_groundToleranceTime;
        }

        // Update ground tolerance timer
        if (m_inputState.groundToleranceTimer >= 0) {
            m_inputState.groundToleranceTimer -= Time.fixedDeltaTime;
        }

        // Cancel jump if the player hits something
        if (m_actor.contacts.above) {
            m_inputState.jump = false;
            m_inputState.cancelJump = false;
            return;
        }

        // Cancel jump if the player releases the jump button
        if (m_inputState.cancelJump) {
            m_inputState.jump = false;
            m_inputState.cancelJump = false;
            m_rb.velocity = new Vector2(m_rb.velocity.x, m_rb.velocity.y * m_jumpCancelSpeedMultiplier);
            return;
        }

        // Update velocity and jump timers
        if (m_inputState.jump) {
            if (m_inputState.jumpCancelTimer >= 0) {
                if (m_inputState.jumpTimer >= 0) {
                    m_rb.velocity = new Vector2(m_rb.velocity.x, m_jumpSpeed + m_momentum.y);
                    m_inputState.jumpTimer -= Time.fixedDeltaTime;
                }
                m_inputState.jumpCancelTimer -= Time.fixedDeltaTime;
            } else {
                m_inputState.jump = false;
            }
        }
    }

    //************************//
    //    Public Functions    //
    //************************//

    // Disables user input so that the rigidbody can be controlled externally.
    public void DisableUserInput(bool _disable)
    {
        if (m_inputDisabled == _disable) {
            return;
        }

        m_inputDisabled = _disable;

        if (_disable) {
            m_momentum = Vector2.zero;
        }

        m_externalVelocity = Vector2.zero;
        m_inputState.Reset();
        m_rb.gravityScale = _disable ? 0 : 1f;
    }
}