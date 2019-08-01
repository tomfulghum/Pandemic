using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaguePeasant : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//
    public enum MovementState
    {
        Decide,
        Idle,
        Move, Sit,
        Attack,
        RangedAttack,
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

    [SerializeField] private float m_chaseRadius = 3f;
    [SerializeField] private float m_movementSpeed = 1f;

    [SerializeField] private float m_minShootDistance = 10f;
    [SerializeField] private float m_projectileSpeed = 15f;
    [SerializeField] private GameObject m_projectile;

    [SerializeField] private Transform m_projectileStartPos;
    [SerializeField] private GameObject m_attackHitBox;

    [SerializeField] private LayerMask m_sightBlockingLayers;

    //ranged attack evtl erst triggern wenn der spieler eine gewisse distanz vom enemy hat
    //CheckGroundAhead //--> bool use intelligent edgemovement?

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

    private MovementState m_currentMovementState = MovementState.Decide; 
    private MovementDirection m_currentMovementDirection = MovementDirection.Left;

    private float m_perceptionRadius = 15f;
    private float m_perceptionAngle = 15f;

    private int m_directionCounter;
    private int m_idleCounter;
    private int m_sitCounter;

    private bool m_rangedAttackActive; //? private?
    private bool m_rangedAttackOnCooldown;

    private Transform m_objectToChase;
    private Transform m_player;

    private Actor2D m_actor;
    private Enemy m_enemy;
    private Rigidbody2D m_rb;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    // Start is called before the first frame update
    void Start()
    {
        m_actor = GetComponent<Actor2D>();
        m_enemy = GetComponent<Enemy>();
        m_rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_enemy.currentEnemyState == Enemy.EnemyState.Moving)
        {
            SetMovementState();
            //Debug.Log(CurrentMovementState);
            switch (m_currentMovementState)
            {
                case MovementState.Move:
                    {
                        if (currentMovementDirection == MovementDirection.Right)
                            m_rb.velocity = Vector2.right * m_movementSpeed;
                        else
                            m_rb.velocity = Vector2.left * m_movementSpeed;

                        m_directionCounter--;
                        if (m_directionCounter < 0)
                        {
                            float IdleEvent = Random.Range(0f, 1f); //ist in dem sinne schlecht das man durch zufall endlos in idle landen kann
                            if (IdleEvent > 0.7) // 0.7
                            {
                                m_rb.velocity = Vector2.zero;
                                m_idleCounter = Random.Range(100, 180);
                                m_currentMovementState = MovementState.Idle;
                                m_directionCounter = 150 + Random.Range(0, 150);
                            }
                            else
                                ChangeDirection();
                        }
                        break;
                    }
                case MovementState.Chase:
                    {
                        if (m_objectToChase.position.x > transform.position.x)
                        {
                            m_currentMovementDirection = MovementDirection.Right;
                            m_rb.velocity = Vector2.right * m_movementSpeed + new Vector2(0, m_rb.velocity.y);
                        }
                        else
                        {
                            m_currentMovementDirection = MovementDirection.Left;
                            m_rb.velocity = Vector2.left * m_movementSpeed + new Vector2(0, m_rb.velocity.y);
                        }
                        break;
                    }
                case MovementState.Idle:
                    {
                        if (m_idleCounter > 150)
                        {
                            m_sitCounter = Random.Range(100, 180);
                            m_currentMovementState = MovementState.Sit;
                        }
                        else
                        {
                            m_idleCounter--;
                            if (m_idleCounter < 0)
                            {
                                m_currentMovementState = MovementState.Decide;
                            }
                        }
                        break;
                    }
                case MovementState.Sit:
                    {
                        m_sitCounter--;
                        if (m_sitCounter < 0)
                        {
                            m_currentMovementState = MovementState.Decide;
                        }
                        break;
                    }
                case MovementState.RangedAttack:
                    {
                        if (m_rangedAttackOnCooldown == false)
                            StartCoroutine(RangedAttack());
                        m_rb.velocity = Vector2.zero;
                        break;
                    }
                case MovementState.Attack:
                    {
                        m_rb.velocity = Vector2.zero;
                        break;
                    }
            }
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    void SetMovementState()
    {
        m_objectToChase = PlayerInSight(); //attack einplanen
        m_player = PlayerInPercetpionRadius();
        if (m_player != null && m_objectToChase == null && m_rangedAttackOnCooldown == false && Vector2.Distance(m_player.position, transform.position) > m_minShootDistance) //nur wenn spieler weit genug von plauge peasant weg ist schießt er
        {
            m_currentMovementState = MovementState.RangedAttack;
            if (m_player.position.x > transform.position.x)
                m_currentMovementDirection = MovementDirection.Right;
            else
                m_currentMovementDirection = MovementDirection.Left;
        }
        else if (m_rangedAttackActive == false)
        {
            if (m_objectToChase != null)
                m_currentMovementState = MovementState.Chase;
            else if (CheckGroundAhead() && currentMovementState != MovementState.Idle && currentMovementState != MovementState.Sit)
                m_currentMovementState = MovementState.Move;
            else if (currentMovementState != MovementState.Idle && currentMovementState != MovementState.Sit)
            {
                ChangeDirection();
                m_currentMovementState = MovementState.Move;
            }
        }
    }

    IEnumerator RangedAttack()
    {
        m_rangedAttackActive = true;
        GetComponent<Animator>().SetTrigger("RangedAttack");
        m_rangedAttackOnCooldown = true;
        yield return new WaitForSeconds(2f);
        m_rangedAttackOnCooldown = false;
    }

    void ShootProjectile() //sollte ein start transform (mundposition bekommen)
    {
        GameObject projectile = Instantiate(m_projectile, m_projectileStartPos.position, m_projectileStartPos.rotation);
        if (currentMovementDirection == MovementDirection.Left)
            projectile.GetComponent<Rigidbody2D>().velocity = Vector2.left * m_projectileSpeed;
        else
            projectile.GetComponent<Rigidbody2D>().velocity = Vector2.right * m_projectileSpeed;
        m_rangedAttackActive = false;
    }


    Transform PlayerInPercetpionRadius()
    {
        Collider2D[] colliderInRange = Physics2D.OverlapCircleAll(transform.position, m_perceptionRadius);
        for (int i = 0; i < colliderInRange.Length; i++)
        {
            if (colliderInRange[i].CompareTag("Player"))
            {
                if (currentMovementDirection == MovementDirection.Left && transform.position.x > colliderInRange[i].transform.position.x)
                {
                    float rayCastLenght = Vector2.Distance(transform.position, colliderInRange[i].transform.position);
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, (colliderInRange[i].transform.position - transform.position), rayCastLenght, m_sightBlockingLayers);
                    Vector2 peasantToPlayer = (colliderInRange[i].transform.position - transform.position).normalized;
                    float angleInDeg = Vector2.Angle(peasantToPlayer, Vector2.left);
                    if (hit == false && angleInDeg < m_perceptionAngle)
                        return colliderInRange[i].transform;
                }
                if (currentMovementDirection == MovementDirection.Right && transform.position.x < colliderInRange[i].transform.position.x)
                {
                    float rayCastLenght = Vector2.Distance(transform.position, colliderInRange[i].transform.position);
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, (colliderInRange[i].transform.position - transform.position), rayCastLenght, m_sightBlockingLayers);
                    Vector2 peasantToPlayer = (colliderInRange[i].transform.position - transform.position).normalized;
                    float angleInDeg = Vector2.Angle(peasantToPlayer, Vector2.right);
                    if (hit == false && angleInDeg < m_perceptionAngle)
                        return colliderInRange[i].transform;
                }
            }
        }
        return null;
    }

    Transform PlayerInSight()
    {
        Collider2D[] colliderInRange = Physics2D.OverlapCircleAll(transform.position, m_chaseRadius);
        for (int i = 0; i < colliderInRange.Length; i++)
        {
            if (colliderInRange[i].CompareTag("Player"))
            {
                float rayCastLenght = Vector2.Distance(transform.position, colliderInRange[i].transform.position);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, (colliderInRange[i].transform.position - transform.position), rayCastLenght, m_sightBlockingLayers);
                if (hit == false)
                    return colliderInRange[i].transform;
            }
        }
        return null;
    }

    void ChangeDirection()
    {
        if (currentMovementDirection == MovementDirection.Left)
            m_currentMovementDirection = MovementDirection.Right;
        else
            m_currentMovementDirection = MovementDirection.Left;
        m_directionCounter = 150 + Random.Range(0, 150);
    }

    bool CheckGroundAhead()
    {
        RaycastHit2D hit;
        if (currentMovementDirection == MovementDirection.Left)
            hit = Physics2D.Raycast(transform.position + Vector3.left, -Vector2.up, GetComponent<Collider2D>().bounds.extents.y + 0.2f);
        else
            hit = Physics2D.Raycast(transform.position + Vector3.right, -Vector2.up, GetComponent<Collider2D>().bounds.extents.y + 0.2f);
        if (hit.collider != null)
            return true;
        return false;
    }
}
