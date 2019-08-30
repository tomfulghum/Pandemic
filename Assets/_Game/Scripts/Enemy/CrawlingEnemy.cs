using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//vllt ausrechnen wo er mit einem potenziellen jump landen würde und dann mit höherer chance springen (wenn er auf einer plattform aufkommen würde)
//requires component enemy
//jump intelligenter machen --> evtl auch absprungwinkel ausrechnen
public class CrawlingEnemy : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    public enum MovementState //in air in falling umändern --> wenn noch ground below --> nichts tun nur gravity applyn
    {
        Decide,
        Move,
        Jump,
        Falling,
        Chase
    }

    public enum MovementDirection //brauch ich none überhaupt?
    {
        None,
        Left,
        Right
    }

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float m_movementSpeed = 1f;
    [SerializeField] private float m_chaseRadius = 3f;
    [SerializeField] private float m_jumpForce = 15f; //um die selbe wurfbahn zu kriegen: doppelte jumpforce = vierfache gravity
    [SerializeField] private bool m_useIntelligentJump = true; // default false? //variable jumpprobability --> if 0 then no jump
    [SerializeField] private bool m_useJump = true; //ändern in eine intelligenz skala von 1 - 10 oder so
    //[SerializeField] private bool m_useRandomJumping = true; //not yet implemented
    [SerializeField] private LayerMask m_sightBlockingLayers = default;

    //******************//
    //    Properties    //
    //******************//

    public MovementState currentMovementState //vllt am anfang auf decide
    {
        get { return m_currentMovementState; }
    }

    public MovementDirection currentMovementDirection
    {
        get { return m_currentMovementDirection; }
    }

    public bool jumping //nur für animation aktuell --> später verbessern
    {
        get { return m_jumping; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private MovementState m_currentMovementState = MovementState.Decide;
    private MovementDirection m_currentMovementDirection = MovementDirection.None;
    private bool m_jumping = false;

    private int m_directionCounter = 0;

    private Transform m_objectToChase = null;
    private Vector2 m_jumpDirection = Vector2.zero;

    private Actor2D m_actor;
    private Rigidbody2D m_rb;
    private Collider2D m_coll;
    private Enemy m_enemy;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    //public GameObject DotPrefab;
    //GameObject DotParent; //only for visuals
    //was passiert wenn du den gegner in der luft triffst?
    //jump values noch anpassen
    void Start()
    {
        m_actor = GetComponent<Actor2D>();
        m_rb = GetComponent<Rigidbody2D>();
        m_coll = GetComponent<Collider2D>();
        m_enemy = GetComponent<Enemy>();
        //DotParent = new GameObject("Parent Dot Enemy"); //only for visuals
    }

    void Update()
    {

        if (m_enemy.currentEnemyState == Enemy.EnemyState.Moving)
        {
            if (currentMovementState == MovementState.Decide)
            { // && CurrentMovementState != MovementState.Falling) //warum?
                SetNextMove();
            }
            if (currentMovementState != MovementState.Decide)
            {
                SetMovementPattern();
            }
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void SetNextMove()
    {
        //jump direction auf default stellen? --> brauchts das überhaupt?
        m_objectToChase = PlayerInSight();
        if (m_objectToChase != null)
        {
            m_currentMovementState = MovementState.Chase;
        }
        else if (!m_actor.contacts.below)
        { //&&CheckGroundBelow
            m_currentMovementState = MovementState.Falling;
        }
        else if (CheckGroundAhead())
        {
            m_currentMovementState = MovementState.Move;
        }
        else if (CheckGroundAhead() == false)
        {
            float rnd = Random.Range(0f, 1f);
            if ((rnd > 0.9f && m_useJump) || (m_useIntelligentJump && CheckIfAnyJumpPossible()))
            { //rnd > 0.9f || //--> for better testing without random
                m_currentMovementState = MovementState.Jump;
            }
            else
            {
                ChangeDirection();
                m_currentMovementState = MovementState.Move;
            }
        }
    }

    private void SetMovementPattern()
    {
        switch (currentMovementState)
        {
            case MovementState.Chase:
                {
                    if (m_objectToChase.position.x > transform.position.x)
                    {
                        m_currentMovementDirection = MovementDirection.Right;
                        m_rb.velocity = new Vector2(m_movementSpeed, m_rb.velocity.y);
                    }
                    else
                    {
                        m_currentMovementDirection = MovementDirection.Left;
                        m_rb.velocity = new Vector2(-m_movementSpeed, m_rb.velocity.y);
                    }
                    m_currentMovementState = MovementState.Decide;
                    break;
                }

            case MovementState.Move:
                {
                    m_directionCounter--;
                    if (m_directionCounter < 0 || m_actor.contacts.left || m_actor.contacts.right)
                    {
                        ChangeDirection();
                    }
                    if (currentMovementDirection == MovementDirection.Right)
                    {
                        m_rb.velocity = Vector2.right * m_movementSpeed + new Vector2(0, m_rb.velocity.y);
                    }
                    else
                    {
                        m_rb.velocity = Vector2.left * m_movementSpeed + new Vector2(0, m_rb.velocity.y);
                    }
                    m_currentMovementState = MovementState.Decide;
                    break;
                }
            case MovementState.Jump:
                {
                    m_rb.velocity = Jump(m_jumpDirection);
                    m_jumping = true;
                    m_directionCounter = 200 + Random.Range(0, 200); //vllt unnötig? oder besser wo anders?
                    m_currentMovementState = MovementState.Decide; //Falling
                    break;
                }
            case MovementState.Falling:
                {
                    //gegner bewegt sich mit seiner velcoity aus move weiter --> irgendwas dagegen tun
                    if (m_actor.contacts.below)
                    { //&&CheckGroundBelow
                        m_jumping = false;
                        m_currentMovementState = MovementState.Decide;
                    }
                    break;
                }
        }
    }

    private void ChangeDirection()
    {
        if (currentMovementDirection == MovementDirection.Left)
        {
            m_currentMovementDirection = MovementDirection.Right;
        }
        else
        {
            m_currentMovementDirection = MovementDirection.Left;
        }
        m_directionCounter = 200 + Random.Range(0, 200);
    }

    private Transform PlayerInSight()
    {
        Collider2D[] colliderInRange = Physics2D.OverlapCircleAll(transform.position, m_chaseRadius);
        for (int i = 0; i < colliderInRange.Length; i++)
        {
            if (colliderInRange[i].CompareTag("Player"))
            {
                float rayCastLength = Vector2.Distance(transform.position, colliderInRange[i].transform.position);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, (colliderInRange[i].transform.position - transform.position), rayCastLength, m_sightBlockingLayers);
                if (hit == false)
                {
                    return colliderInRange[i].transform;
                }
            }
        }
        return null;
    }

    private Vector2 Jump(Vector2 _jumpDirection)
    {
        return new Vector2(_jumpDirection.x, _jumpDirection.y) * m_jumpForce; //10 = jumpforce --> variable erstellen //vllt siehts besser aus wenn er seine aktuelle velocity behält?
    }

    private bool CheckGroundAhead() //if yes --> decide jump or not //layermask? doesnt hit background?
    {
        RaycastHit2D hit;
        if (currentMovementDirection == MovementDirection.Left)
        {
            hit = Physics2D.Raycast(transform.position + Vector3.left, -Vector2.up, m_coll.bounds.extents.y + 0.2f);
        }
        else
        {
            hit = Physics2D.Raycast(transform.position + Vector3.right, -Vector2.up, m_coll.bounds.extents.y + 0.2f);
        }
        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }

    private bool CheckIfAnyJumpPossible() //denke es muss nur noch bisschen an den zahlen geshraubt werden --> was ist mit vector normalisieren
    {
        bool jumpPossible = false;

        if (currentMovementDirection == MovementDirection.Right)
        {
            if (CheckJumpPath(transform.position, new Vector2(Mathf.Cos(60 * Mathf.Deg2Rad), Mathf.Sin(60 * Mathf.Deg2Rad)).normalized * m_jumpForce))
            {
                jumpPossible = true;
                m_jumpDirection = new Vector2(Mathf.Cos(60 * Mathf.Deg2Rad), Mathf.Sin(60 * Mathf.Deg2Rad)).normalized;
            }
            if (CheckJumpPath(transform.position, new Vector2(Mathf.Cos(45 * Mathf.Deg2Rad), Mathf.Sin(45 * Mathf.Deg2Rad)).normalized * m_jumpForce))
            {
                jumpPossible = true;
                m_jumpDirection = new Vector2(Mathf.Cos(45 * Mathf.Deg2Rad), Mathf.Sin(45 * Mathf.Deg2Rad)).normalized;
            }
            if (CheckJumpPath(transform.position, new Vector2(Mathf.Cos(75 * Mathf.Deg2Rad), Mathf.Sin(75 * Mathf.Deg2Rad)).normalized * m_jumpForce))
            {
                jumpPossible = true;
                m_jumpDirection = new Vector2(Mathf.Cos(75 * Mathf.Deg2Rad), Mathf.Sin(75 * Mathf.Deg2Rad)).normalized;
            }
        }
        if (currentMovementDirection == MovementDirection.Left)
        {
            if (CheckJumpPath(transform.position, new Vector2(-Mathf.Cos(60 * Mathf.Deg2Rad), Mathf.Sin(60 * Mathf.Deg2Rad)).normalized * m_jumpForce))
            {
                jumpPossible = true;
                m_jumpDirection = new Vector2(-Mathf.Cos(60 * Mathf.Deg2Rad), Mathf.Sin(60 * Mathf.Deg2Rad)).normalized;
            }
            if (CheckJumpPath(transform.position, new Vector2(-Mathf.Cos(45 * Mathf.Deg2Rad), Mathf.Sin(45 * Mathf.Deg2Rad)).normalized * m_jumpForce))
            {
                jumpPossible = true;
                m_jumpDirection = new Vector2(-Mathf.Cos(45 * Mathf.Deg2Rad), Mathf.Sin(45 * Mathf.Deg2Rad)).normalized;
            }
            if (CheckJumpPath(transform.position, new Vector2(-Mathf.Cos(75 * Mathf.Deg2Rad), Mathf.Sin(75 * Mathf.Deg2Rad)).normalized * m_jumpForce))
            {
                jumpPossible = true;
                m_jumpDirection = new Vector2(-Mathf.Cos(75 * Mathf.Deg2Rad), Mathf.Sin(75 * Mathf.Deg2Rad)).normalized;
            }
        }
        return jumpPossible;
    }

    private bool CheckJumpPath(Vector2 _startPosition, Vector2 _launchVelocity)
    {
        //DotParent.transform.position = _startPosition;
        float timeBetweenDots = 0.08f; //dafür variable im editor erstellen
        int numOfChecks = 0;
        bool hitSmth = false;
        float throwTime = 0f;
        while (hitSmth == false && numOfChecks < 50) //30 = max num of checks
        {
            numOfChecks++;
            Vector2 StartPosition = CalculatePosition(throwTime, _launchVelocity, _startPosition);
            throwTime += timeBetweenDots;
            Vector2 targetPosition = CalculatePosition(throwTime, _launchVelocity, _startPosition);
            float raycastLength = (targetPosition - StartPosition).magnitude;
            RaycastHit2D hit = Physics2D.Raycast(StartPosition, (targetPosition - StartPosition), raycastLength, m_sightBlockingLayers); //anstatt sightblocking vllt movementblocking nehmen
            if (hit.collider != null && hit.collider.transform.position.y <= hit.point.y)
            { //position compare
                hitSmth = true;
            }
            else if (hit.collider != null && hit.collider.transform.position.y > hit.point.y)
            {
                return false;
            }
        }
        return hitSmth;
    }

    private Vector2 CalculatePosition(float _elapsedTime, Vector2 _launchVelocity, Vector2 _initialPosition)
    {
        return Physics2D.gravity * _elapsedTime * _elapsedTime * 0.5f + _launchVelocity * _elapsedTime + _initialPosition;
    }
}
