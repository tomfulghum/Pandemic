using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaguePeasant : MonoBehaviour
{
    public enum MovementState { Decide, Idle, Move, Sit, Attack, RangedAttack, Chase }
    public enum MovementDirection { None, Left, Right }

    private MovementState m_currentMovementState = MovementState.Decide; //vllt am anfang auf decide
    private MovementDirection m_currentMovementDirection = MovementDirection.Left;

    public MovementState currentMovementState
    {
        get { return m_currentMovementState; }
    }

    public MovementDirection currentMovementDirection
    {
        get { return m_currentMovementDirection; }
    }

    public float ChaseRadius = 3f;
    public float MovementSpeed = 1f;
    public float ProjectileSpeed = 15f;
    public GameObject Projectile;
    public GameObject AttackHitBox;
    public Transform ProjectileStartPos;
    //public Transform RightStartPos;
    //ranged attack evtl erst triggern wenn der spieler eine gewisse distanz vom enemy hat
    public LayerMask SightBlockingLayers;

    //CheckGroundAhead //--> bool use intelligent edgemovement?

    float PerceptionRadius = 15f;
    float PerceptionAngle = 15f;

    int DirectionCounter;
    int IdleCounter;
    int SitCounter;

    [HideInInspector] public bool RangedAttackActive;

    bool RangedAttackOnCooldown;
    Transform ObjectToChase;
    Transform Player;

    Actor2D actor;
    Enemy enemy;
    // Start is called before the first frame update
    void Start()
    {
        actor = GetComponent<Actor2D>();
        enemy = GetComponent<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.currentEnemyState == Enemy.EnemyState.Moving)
        {
            SetMovementState();
            //Debug.Log(CurrentMovementState);
            switch (m_currentMovementState)
            {
                case MovementState.Move:
                    {
                        if (currentMovementDirection == MovementDirection.Right)
                            actor.velocity = Vector2.right * MovementSpeed;
                        else
                            actor.velocity = Vector2.left * MovementSpeed;

                        DirectionCounter--;
                        if (DirectionCounter < 0)
                        {
                            float IdleEvent = Random.Range(0f, 1f); //ist in dem sinne schlecht das man durch zufall endlos in idle landen kann
                            if (IdleEvent > 0.7) // 0.7
                            {
                                actor.velocity = Vector2.zero;
                                IdleCounter = Random.Range(100, 180);
                                m_currentMovementState = MovementState.Idle;
                                DirectionCounter = 150 + Random.Range(0, 150);
                            }
                            else
                                ChangeDirection();
                        }
                        break;
                    }
                case MovementState.Chase:
                    {
                        if (ObjectToChase.position.x > transform.position.x)
                        {
                            m_currentMovementDirection = MovementDirection.Right;
                            actor.velocity = Vector2.right * MovementSpeed + new Vector2(0, actor.velocity.y);
                        }
                        else
                        {
                            m_currentMovementDirection = MovementDirection.Left;
                            actor.velocity = Vector2.left * MovementSpeed + new Vector2(0, actor.velocity.y);
                        }
                        break;
                    }
                case MovementState.Idle:
                    {
                        if (IdleCounter > 150)
                        {
                            SitCounter = Random.Range(100, 180);
                            m_currentMovementState = MovementState.Sit;
                        }
                        else
                        {
                            IdleCounter--;
                            if (IdleCounter < 0)
                            {
                                m_currentMovementState = MovementState.Decide;
                            }
                        }
                        break;
                    }
                case MovementState.Sit:
                    {
                        SitCounter--;
                        if (SitCounter < 0)
                        {
                            m_currentMovementState = MovementState.Decide;
                        }
                        break;
                    }
                case MovementState.RangedAttack:
                    {
                        if (RangedAttackOnCooldown == false)
                            StartCoroutine(RangedAttack());
                        actor.velocity = Vector2.zero;
                        break;
                    }
                case MovementState.Attack:
                    {
                        actor.velocity = Vector2.zero;
                        break;
                    }
            }
        }
    }

    void SetMovementState()
    {
        ObjectToChase = PlayerInSight(); //attack einplanen
        Player = PlayerInPercetpionRadius();
        if (Player != null && ObjectToChase == null && RangedAttackOnCooldown == false)
        {
            m_currentMovementState = MovementState.RangedAttack;
            if (Player.position.x > transform.position.x)
                m_currentMovementDirection = MovementDirection.Right;
            else
                m_currentMovementDirection = MovementDirection.Left;
        }
        else if (RangedAttackActive == false)
        {
            if (ObjectToChase != null)
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
        RangedAttackActive = true;
        GetComponent<Animator>().SetTrigger("RangedAttack");
        RangedAttackOnCooldown = true;
        yield return new WaitForSeconds(2f);
        RangedAttackOnCooldown = false;
    }

    void ShootProjectile() //sollte ein start transform (mundposition bekommen)
    {
        GameObject projectile = Instantiate(Projectile, ProjectileStartPos.position, ProjectileStartPos.rotation);
        if (currentMovementDirection == MovementDirection.Left)
            projectile.GetComponent<Actor2D>().velocity = Vector2.left * ProjectileSpeed;
        else
            projectile.GetComponent<Actor2D>().velocity = Vector2.right * ProjectileSpeed;
        RangedAttackActive = false;
    }


    Transform PlayerInPercetpionRadius()
    {
        Collider2D[] ColliderInRange = Physics2D.OverlapCircleAll(transform.position, PerceptionRadius);
        for (int i = 0; i < ColliderInRange.Length; i++)
        {
            if (ColliderInRange[i].CompareTag("Player"))
            {
                if (currentMovementDirection == MovementDirection.Left && transform.position.x > ColliderInRange[i].transform.position.x)
                {
                    float RayCastLenght = Vector2.Distance(transform.position, ColliderInRange[i].transform.position);
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, (ColliderInRange[i].transform.position - transform.position), RayCastLenght, SightBlockingLayers);
                    Vector2 PeasantToPlayer = (ColliderInRange[i].transform.position - transform.position).normalized;
                    float AngleInDeg = Vector2.Angle(PeasantToPlayer, Vector2.left);
                    if (hit == false && AngleInDeg < PerceptionAngle)
                        return ColliderInRange[i].transform;
                }
                if (currentMovementDirection == MovementDirection.Right && transform.position.x < ColliderInRange[i].transform.position.x)
                {
                    float RayCastLenght = Vector2.Distance(transform.position, ColliderInRange[i].transform.position);
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, (ColliderInRange[i].transform.position - transform.position), RayCastLenght, SightBlockingLayers);
                    Vector2 PeasantToPlayer = (ColliderInRange[i].transform.position - transform.position).normalized;
                    float AngleInDeg = Vector2.Angle(PeasantToPlayer, Vector2.right);
                    if (hit == false && AngleInDeg < PerceptionAngle)
                        return ColliderInRange[i].transform;
                }
            }
        }
        return null;
    }

    Transform PlayerInSight()
    {
        Collider2D[] ColliderInRange = Physics2D.OverlapCircleAll(transform.position, ChaseRadius);
        for (int i = 0; i < ColliderInRange.Length; i++)
        {
            if (ColliderInRange[i].CompareTag("Player"))
            {
                float RayCastLenght = Vector2.Distance(transform.position, ColliderInRange[i].transform.position);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, (ColliderInRange[i].transform.position - transform.position), RayCastLenght, SightBlockingLayers);
                if (hit == false)
                    return ColliderInRange[i].transform;
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
        DirectionCounter = 150 + Random.Range(0, 150);
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
