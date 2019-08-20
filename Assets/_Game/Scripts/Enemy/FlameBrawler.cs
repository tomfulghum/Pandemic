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
    [SerializeField] private LayerMask m_lethalObjects = default;
    [SerializeField] private LayerMask m_sightBlockingLayers = default;
    [SerializeField] private float m_timeBetweenFlames = 2f;
    [SerializeField] private GameObject m_flamePrefab = default;

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
    private MovementDirection m_currentMovementDirection = MovementDirection.Right;


    private int m_directionCounter;
    private int m_idleCounter;
    private int m_blockCounter;
    private float m_flameCounter;

    private Transform m_objectToChase;
    private Transform m_lethalObject;

    private Enemy m_enemy;
    private Rigidbody2D m_rb;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {
        m_enemy = GetComponent<Enemy>();
        m_rb = GetComponent<Rigidbody2D>();
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
                        if(m_blockCounter > 60)
                        {
                            m_currentMovementState = MovementState.Decide;
                            m_enemy.invincible = false;
                            m_blockCounter = 0;
                        }
                        break;
                    }
                    //case MovementState.Attack:
                    //    {
                    //        m_rb.velocity = Vector2.zero;
                    //        break;
                    //    }
            }

            if(m_flameCounter > m_timeBetweenFlames)
            {
                m_flameCounter = 0;
                SpawnFlame();
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
        if (m_lethalObject != null)
        {
            if (m_lethalObject.position.x < GetComponent<BoxCollider2D>().bounds.center.x)
                m_currentMovementDirection = MovementDirection.Left;
            else
                m_currentMovementDirection = MovementDirection.Right;
            m_enemy.invincible = true;
            m_currentMovementState = MovementState.Block;
        }
        else if(m_currentMovementState != MovementState.Block)
        {
            if (m_objectToChase != null)
            {
                m_currentMovementState = MovementState.Chase;
            }
            else if(m_currentMovementState != MovementState.Idle)
            {
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
        Vector2 spawnPosition = new Vector2(transform.position.x - GetComponent<BoxCollider2D>().bounds.extents.x, transform.position.y - GetComponent<BoxCollider2D>().bounds.extents.y);
        GameObject flame = Instantiate(m_flamePrefab, spawnPosition, transform.rotation);
        //flame.transform.position = new Vector2(transform.position.x, transform.position.y + flame.GetComponent<BoxCollider2D>().bounds.extents.y / 2);
    }
}
