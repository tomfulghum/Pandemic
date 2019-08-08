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

    [SerializeField] private bool m_dontMove = false;

    [SerializeField] private float m_chaseRadius = 3f;
    [SerializeField] private float m_movementSpeed = 1f;

    [SerializeField] private float m_minShootDistance = 10f;
    [SerializeField] private List<int> m_AimAngles = default; // überarbeiten zu neuenm system
    [SerializeField] private List<int> m_ThrowForces = default;

    //[SerializeField] private float m_minThrowAngle = 30; //not yet implemented
    //[SerializeField] private float m_maxThrowAngle = 60;

    [SerializeField] private GameObject m_projectile = default;
    [SerializeField] private GameObject m_pickUpProjectile = default;

    [SerializeField] private Transform m_projectileStartPos = default;
    //[SerializeField] private GameObject m_attackHitBox = default;

    [SerializeField] private LayerMask m_sightBlockingLayers = default;

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

    private float m_rangedAnimSpeedMultiplier = 1f;
    private float m_rangedAttackSin = 1f;
    private float m_rangedAttackAnimSpeed = 2f;

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
        GetComponent<Animator>().SetFloat("RangedAttackSpeed", m_rangedAttackAnimSpeed);
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
            //ShootProjectile(); //test für targeting
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
        GetComponent<Animator>().SetFloat("RangedAttackSpeed", m_rangedAttackAnimSpeed + (Mathf.Sin(m_rangedAttackSin) * m_rangedAnimSpeedMultiplier));
        m_rangedAttackSin += 0.1f;


        GameObject projectile = Instantiate(m_projectile, m_projectileStartPos.position, m_projectileStartPos.rotation);
        projectile.GetComponent<Rigidbody2D>().velocity = CaculateInitialVelocity(m_projectileTargetPosition);
        projectile.GetComponent<EnemyProjectile>().ApplySpeedMultiplier();

        /*
        if (Random.Range(0f, 1f) < 0.7f)
        {
            GameObject projectile = Instantiate(m_projectile, m_projectileStartPos.position, m_projectileStartPos.rotation);
            projectile.GetComponent<Rigidbody2D>().velocity = CalculateOptimalThrow(m_projectileTargetPosition);
            projectile.GetComponent<EnemyProjectile>().ApplySpeedMultiplier();
        }
        else
        {
            Instantiate(m_pickUpProjectile, m_projectileStartPos.position, m_projectileStartPos.rotation).GetComponent<Rigidbody2D>().velocity = CalculateOptimalThrow(m_projectileTargetPosition);
        }
        */
        m_rangedAttackActive = false;
    }

    
    private float CalculateThrowVelocity(float _throwAngle)
    {
        float horizontalDistance = Mathf.Abs(m_projectileStartPos.position.x - m_projectileTargetPosition.x);
        float verticalDistance = Mathf.Abs(m_projectileStartPos.position.y - m_projectileTargetPosition.y);
        float initialVelocity = horizontalDistance * Mathf.Sqrt(-Physics2D.gravity.y / 2 * (horizontalDistance * Mathf.Tan(_throwAngle * Mathf.Deg2Rad) - verticalDistance)) / Mathf.Cos(_throwAngle * Mathf.Deg2Rad);
       // float initialVelocity = Mathf.Sqrt(-Physics2D.gravity.y * Mathf.Pow(m_projectileTargetPosition.x, 2) / (2 * Mathf.Pow(Mathf.Cos(_throwAngle * Mathf.Deg2Rad), 2) * (m_projectileTargetPosition.y - m_projectileTargetPosition.x * Mathf.Tan(_throwAngle * Mathf.Deg2Rad))));
        return initialVelocity;
    }

    //max range nicht vergessen und evtl checken ob das ziel erreichbar ist

    private Vector2 CaculateInitialVelocity(Vector2 _targetPosition) //wahrscheinlich bei x einfach die distanz nicht math abs machen und damit die richtung herausbekommen
    {
        float horizontalDistance = Mathf.Abs(m_projectileStartPos.position.x - _targetPosition.x);
        float initialVelocity = Mathf.Sqrt(horizontalDistance * (-Physics2D.gravity.y / 2));
        float horizontalVectorMultiplier = Mathf.Sqrt(2) * initialVelocity;
        //Debug.Log("initial Velocity: " + initialVelocity);
        //Debug.Log("initial Velocity final : " + horizontalVectorMultiplier);


        float airTime = -2 / Physics2D.gravity.y * initialVelocity;
        float verticalDistance = Mathf.Abs(m_projectileStartPos.position.y - _targetPosition.y);


        float angle = Vector2.Angle(m_projectileStartPos.position, _targetPosition);
        //Debug.Log("angle: " + angle);
        float airTimeWithHeightDifference = 2 * initialVelocity * Mathf.Sin((45 - angle) * Mathf.Deg2Rad) / -Physics2D.gravity.y * Mathf.Cos(angle * Mathf.Deg2Rad); //test 1                                                                                                                                                       //Debug.Log("airtime: " + airTime);

        //float verticalDistance = Mathf.Abs(m_projectileStartPos.position.y - _targetPosition.y);
        float additionalVerticalVelocity = initialVelocity + (verticalDistance / airTimeWithHeightDifference);
        float verticalVectorMultiplier = Mathf.Sqrt(2) * additionalVerticalVelocity;
        //Debug.Log("additional velocity: " + additionalVerticalVelocity);

        Vector2 throwVelocity = new Vector2(0.5f, 0.5f).normalized;
        throwVelocity.x *= horizontalVectorMultiplier;
        throwVelocity.y *= verticalVectorMultiplier;

        return throwVelocity;
    }

    private Vector2 CalculateOptimalThrow(Vector2 _targetPosition)
    {
        Vector2 launchVelocity = Vector2.zero;
        float minDistanceToPlayer = Mathf.Infinity;
        foreach (int force in m_ThrowForces)
        {
            foreach (int angle in m_AimAngles)
            {
                if (currentMovementDirection == MovementDirection.Right)
                {
                    Vector2 landingPosition = CalculateThrowLandingPosition(m_projectileStartPos.position, new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized * force);
                    if (Vector2.Distance(landingPosition, _targetPosition) < minDistanceToPlayer)
                    {
                        launchVelocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized * force;
                        minDistanceToPlayer = Vector2.Distance(landingPosition, _targetPosition);
                    }
                }
                else
                {
                    Vector2 landingPosition = CalculateThrowLandingPosition(m_projectileStartPos.position, new Vector2(-Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized * force);
                    if (Vector2.Distance(landingPosition, _targetPosition) < minDistanceToPlayer)
                    {
                        launchVelocity = new Vector2(-Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized * force;
                        minDistanceToPlayer = Vector2.Distance(landingPosition, _targetPosition);
                    }
                }
            }
        }
        return launchVelocity;
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

    private Vector2 CalculateThrowLandingPosition(Vector2 _startPosition, Vector2 _launchVelocity)
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
            if (hit.collider != null)
            {
                return hit.point;
            }
        }
        return Vector2.zero; ;
    }

    private Vector2 CalculatePosition(float _elapsedTime, Vector2 _launchVelocity, Vector2 _initialPosition)
    {
        return Physics2D.gravity * _elapsedTime * _elapsedTime * 0.5f + _launchVelocity * _elapsedTime + _initialPosition;
    }
}
