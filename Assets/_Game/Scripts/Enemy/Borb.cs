using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borb : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    public enum MovementState
    {
        Decide,
        Move,
        FlyUp,
        Nosedive,
        Dazed,
        Chase
    }
    public enum MovementDirection
    {
        None,
        Left,
        Right
    }

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float m_chaseRange = 3f; //evlt reicht coneangle
    [SerializeField] private float m_coneAngle = 35;

    [SerializeField] private float m_movementSpeed = 3f;
    [SerializeField] private float m_diveSpeed = 20f;
    [SerializeField] private float m_verticalSpeed = 1.5f;

    [SerializeField] private float m_stunTime = 2f;

    [SerializeField] private float m_targetFlightHeight = 8f;
    [SerializeField] private bool m_useHeightAdjustments = false;

    [SerializeField] private LayerMask m_sightBlockingLayers = default;
    [SerializeField] private BoxCollider2D m_knockBackCollider;

    //******************//
    //    Properties    //
    //******************//

    public MovementState currentMovementState
    {
        get { return m_currentMovementState; }
    }

    public MovementDirection currentMovementDirection
    {
        get { return m_currentMovementDirection; }
    }


    //**********************//
    //    Private Fields    //
    //**********************//

    private MovementState m_currentMovementState = MovementState.Decide; //vllt am anfang auf decide
    private MovementDirection m_currentMovementDirection = MovementDirection.Left;

    private float m_currentStunTime;
    private int m_directionCounter;

    private float m_flightHeight; //bei knockback zurück auf die flight height //--> change in height above ground
    private float m_diveTriggerRange = 0.2f;

    private Transform m_objectToChase;
    private Actor2D m_actor;
    private Enemy m_enemy;
    private Rigidbody2D m_rb;
    private EnemyKnockback m_ekb;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    // Start is called before the first frame update
    void Start()
    {
        m_currentStunTime = m_stunTime;
        m_flightHeight = transform.position.y;
        SetFlightHeight();
        m_actor = GetComponent<Actor2D>();
        m_enemy = GetComponent<Enemy>();
        m_rb = GetComponent<Rigidbody2D>();
        m_ekb = GetComponentInChildren<EnemyKnockback>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_enemy.currentEnemyState == Enemy.EnemyState.Moving)
        {
            SetMovementState();
            VisualizeChaseCone();
            if (m_useHeightAdjustments)
                SetFlightHeight();

            switch (m_currentMovementState)
            {
                case MovementState.Move: //irgendwo evtl noch des stuck einbauen
                    {
                        //CheckFlightHeight();
                        m_directionCounter--;
                        if (m_directionCounter < 0 || m_actor.contacts.left || m_actor.contacts.right)
                            ChangeDirection();
                        FlyInDirection();
                        if (CheckFlightHeight() == false && CheckCeilingHit() == false)
                            AdjustFlightHeight();
                        break;
                    }
                case MovementState.Chase:
                    {
                        if (m_objectToChase.position.x < transform.position.x)
                            m_currentMovementDirection = MovementDirection.Left;
                        else
                            m_currentMovementDirection = MovementDirection.Right;
                        FlyInDirection();
                        break;
                    }
                case MovementState.Nosedive:
                    {
                        if (CheckPlayerHit())
                        {
                            m_currentMovementState = MovementState.FlyUp;
                            m_ekb.IsEnemyLethal(false);
                        }
                        if (CheckGroundHit() == false)
                            m_rb.velocity = Vector2.down * m_diveSpeed;
                        else
                        {
                            m_currentStunTime = m_stunTime;
                            m_currentMovementState = MovementState.Dazed;
                            m_ekb.IsEnemyLethal(false);
                        }
                        //CurrentMovementState = MovementState.FlyUp;
                        break;
                    }
                case MovementState.Dazed:
                    {
                        m_currentStunTime -= Time.deltaTime;
                        if (m_currentStunTime < 0)
                            m_currentMovementState = MovementState.FlyUp;
                        break;
                    }
            }
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void SetFlightHeight()
    {
        float distanceToGround = GetDistanceToGround(new Vector2(transform.position.x, m_flightHeight));
        if (distanceToGround != m_targetFlightHeight)
        {
            float heightDifference = Mathf.Abs(distanceToGround - m_targetFlightHeight);
            if (distanceToGround <= m_targetFlightHeight)
                m_flightHeight += heightDifference;
            else
                m_flightHeight -= heightDifference;
        }
    }

    private void VisualizeChaseCone()
    {
        Vector2 directionLine = Vector2.down * GetDistanceToGround(transform.position);
        Vector2 leftArc = RotateVector(directionLine, m_coneAngle);
        Vector2 rightArc = RotateVector(directionLine, -m_coneAngle);

        Debug.DrawLine(transform.position, (Vector2)transform.position + directionLine);
        Debug.DrawLine(transform.position, (Vector2)transform.position + leftArc);
        Debug.DrawLine(transform.position, (Vector2)transform.position + rightArc);
    }

    private bool CheckGroundHit()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, m_diveSpeed * Time.deltaTime + GetComponent<Collider2D>().bounds.extents.y, m_sightBlockingLayers); //evlt anstatt 1 die distanz berechnen die er in dem frame zurückgelegt hat //divespeed * Time.deltatime //oder evtl if distance to ground <= 0.1f / 0 oder so
        if (hit.collider != null)
            return true;
        return false;
    }

    private bool CheckCeilingHit()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, m_verticalSpeed * Time.deltaTime + GetComponent<Collider2D>().bounds.extents.y, m_sightBlockingLayers); //anstatt dive speed fly up speed
        if (hit.collider != null)
            return true;
        return false;
    }


    private bool CheckPlayerHit() //funktioniert nocht nicht
    {
        Debug.Log(PlayerHook.CurrentPlayerState);
        if(m_objectToChase != null && PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Disabled) //funktioniert nicht wenn borb aktuell nosedive macht und ein anderer gegner den spieler getroffen hat --> aber fürs erste ein quick fix
        {
            return true;
        }
        return false;
        /*
        Vector2 ColliderBox = new Vector2(m_knockBackCollider.size.x * m_knockBackCollider.transform.localScale.x, m_knockBackCollider.size.y * m_knockBackCollider.transform.localScale.y);
        Collider2D[] col = Physics2D.OverlapBoxAll(m_knockBackCollider.transform.position, ColliderBox, 0); 
        Debug.Log(col.Length);
        foreach (Collider2D collider in col)
        {
            Debug.Log(collider.gameObject);
            if (collider.CompareTag("Player"))
            {
                Debug.Log("hit player");
                return true;
            }
        }
        return false;
        */
    }

    private float GetDistanceToGround(Vector3 _position)
    {
        float Distance = -1f;
        RaycastHit2D hit = Physics2D.Raycast(_position, Vector2.down, Mathf.Infinity, m_sightBlockingLayers); //evtl eigene layermask //hier nur für ground layer benötigt
        if (hit.collider != null)
            Distance = hit.distance;
        return Distance;
    }

    private void FlyInDirection()
    {
        if (m_currentMovementDirection == MovementDirection.Right)
            m_rb.velocity = Vector2.right * m_movementSpeed;
        else
            m_rb.velocity = Vector2.left * m_movementSpeed;
    }

    private void SetMovementState()
    {
        if (m_currentMovementState != MovementState.Nosedive && m_currentMovementState != MovementState.Dazed) //später ändern
        {
            m_objectToChase = PlayerInSight();
            if (m_objectToChase != null && ChasePlayer() && (transform.position.y == m_flightHeight || CheckCeilingHit()))
                if (Mathf.Abs(transform.position.x - m_objectToChase.position.x) < m_diveTriggerRange)
                {
                    m_currentMovementState = MovementState.Nosedive;
                    m_ekb.IsEnemyLethal(true);
                }
                else
                    m_currentMovementState = MovementState.Chase;
            else
                m_currentMovementState = MovementState.Move;
        }
    }

    private bool ChasePlayer()
    {
        if (Mathf.Abs(m_objectToChase.position.x - transform.position.x) < m_chaseRange)
            return true;
        return false;
    }

    private Transform PlayerInSight()
    {
        Collider2D[] colliderInRange = Physics2D.OverlapCircleAll(transform.position, 10); //später evtl auch besser machen GetDistanceToGround()
        for (int i = 0; i < colliderInRange.Length; i++)
        {
            if (colliderInRange[i].CompareTag("Player"))
            {
                float rayCastLenght = Vector2.Distance(transform.position, colliderInRange[i].transform.position);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, (colliderInRange[i].transform.position - transform.position), rayCastLenght, m_sightBlockingLayers); //hier auch sight block?
                Vector2 borbToPlayer = (colliderInRange[i].transform.position - transform.position).normalized;
                float angleInDeg = Vector2.Angle(borbToPlayer, Vector2.down);
                if (hit == false && angleInDeg < m_coneAngle)
                    return colliderInRange[i].transform;
            }
        }
        return null;
    }

    private void ChangeDirection()
    {
        if (m_currentMovementDirection == MovementDirection.Left)
            m_currentMovementDirection = MovementDirection.Right;
        else
            m_currentMovementDirection = MovementDirection.Left;
        m_directionCounter = 150 + Random.Range(0, 100);
    }

    private void AdjustFlightHeight()
    {
        if (Mathf.Abs(transform.position.y - m_flightHeight) < 0.1f) //könnte gefährlich werden falls der plague flyer mehr als 0.1f pro frame zurücklegen kann
            transform.position = new Vector3(transform.position.x, m_flightHeight, transform.position.z);
        if (transform.position.y != m_flightHeight)
        {
            if (transform.position.y < m_flightHeight)
                m_rb.velocity = new Vector2(m_rb.velocity.x, m_rb.velocity.y + m_verticalSpeed);
            else
                m_rb.velocity = new Vector2(m_rb.velocity.x, m_rb.velocity.y - m_verticalSpeed);
        }
    }

    private bool CheckFlightHeight() //funktioniert nur deshalb weil kurz davor die velocity neu gesetzt wird
    {
        if (transform.position.y != m_flightHeight)
            return false;
        return true;
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}
