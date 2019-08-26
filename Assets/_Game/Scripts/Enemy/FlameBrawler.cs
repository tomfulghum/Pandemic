using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameBrawler : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//
    public enum MovementState
    {
        Decide,
        Idle,
        Move,
        Attack,
        AttackFinished,
        Stuck,
        Block,
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

    [SerializeField] private float m_movementSpeed = 1f;
    [SerializeField] private float m_chaseRadius = 3f;
    [SerializeField] private float m_blockRange = 3f;
    [SerializeField] private float m_attackRange = 2f;
    [SerializeField] private LayerMask m_lethalObjects = default;
    [SerializeField] private LayerMask m_sightBlockingLayers = default;
    [SerializeField] private bool m_leaveFlameTrail = true;
    [SerializeField] private float m_timeBetweenFlames = 2f;
    [SerializeField] private float m_flameLifeTime = 10f;
    [SerializeField] private float m_timeToRegainShield = 5f;
    [SerializeField] private GameObject m_flamePrefab = default;
    [SerializeField] private GameObject m_shieldPrefab = default;

    //******************//
    //    Properties    //
    //******************//

    public MovementState currentMovementState
    {
        get { return m_currentMovementState; }
        set { m_currentMovementState = value; }
    }

    public MovementDirection currentMovementDirection
    {
        get { return m_currentMovementDirection; }
    }

    public bool vulnerable
    {
        get { return m_vulnerable; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private MovementState m_currentMovementState = MovementState.Decide;
    private MovementDirection m_currentMovementDirection = MovementDirection.Right;


    private int m_directionCounter;
    private int m_idleCounter;
    private int m_blockCounter;
    private int m_stuckCounter;
    private float m_flameCounter;
    private float m_regainCounter;

    private float m_flameSpawnXOffset = 2f;

    private bool m_vulnerable = false;
    private bool m_shieldDropped = false;

    private Transform m_objectToChase;
    private Transform m_lethalObject;

    private Vector2 m_droppedShieldPosition;

    private GameObject m_shield;

    private Actor2D m_actor;
    private Enemy m_enemy;
    private Rigidbody2D m_rb;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {
        m_actor = GetComponent<Actor2D>();
        m_enemy = GetComponent<Enemy>();
        m_rb = GetComponent<Rigidbody2D>();
        //m_droppedShieldPosition = m_shield.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_enemy.currentEnemyState == Enemy.EnemyState.Moving)
        {
            SetMovementState();
            switch (m_currentMovementState)
            {
                case MovementState.Move:
                    {
                        m_flameCounter += Time.deltaTime;
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
                        m_flameCounter += Time.deltaTime;
                        if (Mathf.Abs(m_objectToChase.position.x - transform.position.x) > 0.15f) //evtl 0.15f als variable ? oder auch größer machen
                        {
                            m_directionCounter = 150 + Random.Range(0, 150);
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
                        }
                        else
                            m_rb.velocity = Vector2.zero;
                        break;
                    }
                case MovementState.Idle:
                    {
                        m_flameCounter += Time.deltaTime;
                        m_idleCounter--;
                        if (m_idleCounter < 0)
                        {
                            m_currentMovementState = MovementState.Decide;
                        }
                        break;
                    }
                case MovementState.Block: //vllt für eine gewisse zeit im block bleiben --> auch wenn kein gegenstand mehr in der nähe ist
                    {
                        m_rb.velocity = Vector2.zero;
                        m_blockCounter++;
                        if (m_blockCounter > 60)
                        {
                            m_currentMovementState = MovementState.Decide;
                            m_enemy.invincible = false;
                            m_blockCounter = 0;
                        }
                        break;
                    }
                case MovementState.Attack:
                    {
                        m_rb.velocity = Vector2.zero;
                        break;
                    }
                case MovementState.Stuck:
                    {
                        m_stuckCounter--;
                        if(m_stuckCounter < 0)
                        {
                           // m_currentMovementState = MovementState.Decide;
                            GetComponent<FlameBrawlerAnim>().StuckSuccesful();
                            m_stuckCounter = 90; // temporary to fix stuck succes bug 
                        }
                        if(m_shield != null && m_shield .GetComponent<ThrowableObject>().currentObjectState != ThrowableObject.ThrowableState.Inactive)
                        {
                            m_vulnerable = true;
                        }
                        break;
                    }
            }

            if (m_flameCounter > m_timeBetweenFlames)
            {
                m_flameCounter = 0;
                if (m_leaveFlameTrail) //to prevent number overflow
                    SpawnFlame();
            }
            if(m_shieldDropped) //regain shield time muss noch resettet werden (bei drop shield)
            {
                m_regainCounter += Time.deltaTime;
                if(m_regainCounter > m_timeToRegainShield)
                {
                    RegainShield();
                    m_regainCounter = 0;
                }
            }
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void SetMovementState()
    {
        m_objectToChase = PlayerInSight();
        m_lethalObject = LethalObjectInRange();

        if (m_lethalObject != null && m_vulnerable == false)
        {
            if (m_lethalObject.position.x < GetComponent<BoxCollider2D>().bounds.center.x)
                m_currentMovementDirection = MovementDirection.Left;
            else
                m_currentMovementDirection = MovementDirection.Right;
            m_enemy.invincible = true;
            m_currentMovementState = MovementState.Block;
            RegainShield();
        }
        else if (m_currentMovementState != MovementState.Block && m_currentMovementState != MovementState.Attack && m_currentMovementState != MovementState.Stuck && m_currentMovementState != MovementState.AttackFinished)
        {
            if (m_objectToChase != null)
            {
                if (Vector2.Distance(m_objectToChase.position, transform.position) < m_attackRange)
                {
                    m_currentMovementState = MovementState.Attack;
                }
                else
                {
                    m_currentMovementState = MovementState.Chase;
                }
            }
            else if (m_currentMovementState != MovementState.Idle)
            {
                if (CheckGroundAhead() == false || m_actor.contacts.right || m_actor.contacts.left)
                {
                    ChangeDirection();
                    m_currentMovementState = MovementState.Move;
                }
                else
                    m_currentMovementState = MovementState.Move;
            }
        }
    }

    private Transform LethalObjectInRange() //return transform --> turn in right direction
    {
        Collider2D[] hookPointsInRange = Physics2D.OverlapCircleAll(transform.position, m_blockRange, m_lethalObjects);
        foreach (Collider2D col in hookPointsInRange)
        {
            if (col.CompareTag("Throwable") && col.GetComponent<ThrowableObject>().currentObjectState == ThrowableObject.ThrowableState.Thrown) //&& col.GetComponent<ThrowableObject>(). ... == thrown
            {
                return col.transform;
            }
        }
        return null;
    }

    private Transform PlayerInSight()
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

    private void ChangeDirection()
    {
        if (currentMovementDirection == MovementDirection.Left)
            m_currentMovementDirection = MovementDirection.Right;
        else
            m_currentMovementDirection = MovementDirection.Left;
        m_directionCounter = 150 + Random.Range(0, 150);
    }

    private void SpawnFlame()
    {
        float xOffset = m_flameSpawnXOffset;
        if (m_currentMovementDirection == MovementDirection.Right)
            xOffset *= -1f;
        Vector2 spawnPosition = new Vector2(transform.position.x + xOffset, transform.position.y + GetComponent<BoxCollider2D>().bounds.max.y / 2);
        GameObject flame = Instantiate(m_flamePrefab, spawnPosition, transform.rotation);
        Destroy(flame, m_flameLifeTime);
    }

    private bool CheckGroundAhead()
    {
        //Debug.DrawRay(GetComponent<BoxCollider2D>().bounds.center + Vector3.left, -Vector3.up * ((GetComponent<Collider2D>().bounds.extents.y) + 0.2f), Color.green);
        RaycastHit2D hit;
        if (currentMovementDirection == MovementDirection.Left)
            hit = Physics2D.Raycast(GetComponent<BoxCollider2D>().bounds.center + Vector3.left, Vector2.down, GetComponent<Collider2D>().bounds.extents.y + 0.2f); //warum nicht Vector2.down?
        else
            hit = Physics2D.Raycast(GetComponent<BoxCollider2D>().bounds.center + Vector3.right, Vector2.down, GetComponent<Collider2D>().bounds.extents.y + 0.2f);
        if (hit.collider != null)
            return true;
        return false;
    }

    private void RegainShield()
    {
        if(m_shield != null) //muss noch gedroppt werden für den spieler
        {
            m_shield.GetComponent<ThrowableObject>().DestroyThrowableObject();
        }
        m_vulnerable = false;
        m_shieldDropped = false;
        //player regain anim
    }

    private void DropShield()
    {
        if (m_shield != null)
        {
            m_shield.GetComponent<ThrowableObject>().DestroyThrowableObject();
        }
        Vector2 shieldPosition = GetComponent<BoxCollider2D>().bounds.center;
        shieldPosition.y -= GetComponent<BoxCollider2D>().bounds.extents.y;
        //m_shield = Instantiate(m_shieldPrefab, transform.position, transform.rotation);
        m_shield = Instantiate(m_shieldPrefab, shieldPosition, transform.rotation);
        m_shieldDropped = true;
        m_regainCounter = 0;
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void Stuck()
    {
        DropShield();
        m_currentMovementState = MovementState.Stuck;
        m_stuckCounter = 90;
    }

    public void TryPickUpShield()
    {
        if (m_vulnerable == false && m_shield != null)
        {
            //m_shield.GetComponent<ThrowableObject>().DestroyThrowableObject(); //destroy with destruction effect
            Destroy(m_shield);
            m_vulnerable = false;
            m_shieldDropped = false;
        }
    }
}
