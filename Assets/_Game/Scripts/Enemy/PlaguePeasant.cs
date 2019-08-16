﻿using System.Collections;
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

    [SerializeField] private bool m_dontMove = false;

    [SerializeField] private float m_chaseRadius = 3f;
    [SerializeField] private float m_movementSpeed = 1f;

    [SerializeField] private float m_minShootDistance = 10f;
    [SerializeField] private float m_targetPositionRadius = 3;

    [SerializeField] private GameObject m_projectile = default;
    [SerializeField] private GameObject m_pickUpProjectile = default;

    [SerializeField] private Transform m_projectileStartPos = default;
    //[SerializeField] private GameObject m_attackHitBox = default;

    [SerializeField] private LayerMask m_sightBlockingLayers = default;

    [SerializeField] private float m_rangedAnimBaseSpeed = 2f;
    [SerializeField] private float m_rangedAnimSpeedMultiplier = 1f;

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
    private MovementDirection m_currentMovementDirection = MovementDirection.Right;

    private float m_perceptionRadius = 30f;
    private float m_perceptionAngle = 15f;

    private int m_directionCounter;
    private int m_idleCounter;
    private int m_sitCounter;

    private float m_rangedAttackSin = 1f;

    private bool m_rangedAttackActive; //? private?
    private bool m_rangedAttackOnCooldown;

    //radius of circle
    private Vector2 m_projectileTargetPosition;

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
        GetComponent<Animator>().SetFloat("RangedAttackSpeed", m_rangedAnimBaseSpeed);
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
                        m_currentMovementState = MovementState.Decide;
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

    private void SetMovementState()
    {
        m_objectToChase = PlayerInSight(); //attack einplanen
        m_player = PlayerInPercetpionRadius();
        if (m_player != null && m_objectToChase == null && m_rangedAttackOnCooldown == false && Vector2.Distance(m_player.position, transform.position) > m_minShootDistance) //nur wenn spieler weit genug von plauge peasant weg ist schießt er
        {
            m_projectileTargetPosition = m_player.transform.position;
            m_currentMovementState = MovementState.RangedAttack;
            if (m_player.position.x > transform.position.x)
                m_currentMovementDirection = MovementDirection.Right;
            else
                m_currentMovementDirection = MovementDirection.Left;
        }
        else if (m_rangedAttackActive == false && m_dontMove == false)
        {
            if (m_objectToChase != null)
                m_currentMovementState = MovementState.Chase;
            else if (currentMovementState != MovementState.Idle && currentMovementState != MovementState.Sit)
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

    private IEnumerator RangedAttack() //ändern --> auf animation speed umstellen
    {
        m_rangedAttackActive = true;
        GetComponent<Animator>().SetTrigger("RangedAttack");
        m_rangedAttackOnCooldown = true;
        //yield return new WaitForSeconds(2f);
        yield return new WaitForSeconds(0.001f);
        m_rangedAttackOnCooldown = false;
    }

    private void ShootProjectile() //sollte ein start transform (mundposition bekommen)
    {
        float shootingSpeed = m_rangedAnimBaseSpeed + (Mathf.Sin(m_rangedAttackSin) * m_rangedAnimSpeedMultiplier);
        if (shootingSpeed == 0)
            shootingSpeed += 0.1f;
        GetComponent<Animator>().SetFloat("RangedAttackSpeed", shootingSpeed);

        //GetComponent<VisualizeTrajectory>().RemoveVisualDots(); //visualize trajectory später wieder entfernen
        //GetComponent<VisualizeTrajectory>().VisualizeDots(m_projectileStartPos.position, CaculateInitialVelocity(SetRandomTargetPoint(m_projectileTargetPosition)));
        GameObject projectile = Instantiate(m_projectile, m_projectileStartPos.position, m_projectileStartPos.rotation);
        projectile.GetComponent<Rigidbody2D>().velocity = CaculateInitialVelocity(SetRandomTargetPoint(m_projectileTargetPosition));
        projectile.GetComponent<EnemyProjectile>().ApplySpeedMultiplier();

      
        //if (Random.Range(0f, 1f) < 0.7f)
        //{
        //    GameObject projectile = Instantiate(m_projectile, m_projectileStartPos.position, m_projectileStartPos.rotation);
        //    projectile.GetComponent<Rigidbody2D>().velocity = CalculateOptimalThrow(m_projectileTargetPosition);
        //    projectile.GetComponent<EnemyProjectile>().ApplySpeedMultiplier();
        //}
        //else
        //{
        //    Instantiate(m_pickUpProjectile, m_projectileStartPos.position, m_projectileStartPos.rotation).GetComponent<Rigidbody2D>().velocity = CalculateOptimalThrow(m_projectileTargetPosition);
        //}
       
        m_rangedAttackActive = false;
    }

    //max range nicht vergessen und evtl checken ob das ziel erreichbar ist

    private Vector2 SetRandomTargetPoint(Vector2 _playerPosition)
    {
        Vector2 targetPoint = _playerPosition + Random.insideUnitCircle * m_targetPositionRadius;
        return targetPoint;
    }

    private Vector2 CaculateInitialVelocity(Vector2 _targetPosition) //wahrscheinlich bei x einfach die distanz nicht math abs machen und damit die richtung herausbekommen
    {
        float horizontalDistance = Mathf.Abs(m_projectileStartPos.position.x - _targetPosition.x);
        float verticalDistance = m_projectileStartPos.position.y - _targetPosition.y;

        float initialVelocity = Mathf.Sqrt(horizontalDistance * (-Physics2D.gravity.y / 2));


        Vector2 throwVelocity = new Vector2(initialVelocity, initialVelocity);

        float airTime = 2.0f * initialVelocity / Physics2D.gravity.y;
        throwVelocity.y += verticalDistance / airTime;

        if (_targetPosition.x < m_projectileStartPos.position.x)
            throwVelocity.x *= -1;

        return throwVelocity;
    }

    private Transform PlayerInPercetpionRadius()
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

    private bool CheckGroundAhead()
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
