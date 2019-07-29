using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaguePeasant : MonoBehaviour
{
    public enum MovementState { Decide, Idle, Move, Sit, Attack, RangedAttack, Chase }
    public enum MovementDirection { None, Left, Right }

    [HideInInspector] public MovementState CurrentMovementState = MovementState.Decide; //vllt am anfang auf decide
    [HideInInspector] public MovementDirection CurrentMovementDirection = MovementDirection.Left;

    public float ChaseRadius = 3f;
    public float MovementSpeed = 1f;
    public float ProjectileSpeed = 15f;
    public GameObject Projectile;
    public Transform LeftStartPos;
    public Transform RightStartPos;
    public LayerMask SightBlockingLayers;

    //CheckGroundAhead //--> bool use intelligent edgemovement?

    float PerceptionRadius = 15f;

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
        if (enemy.CurrentEnemyState == Enemy.EnemyState.Moving)
        {
            SetMovementState();
            //Debug.Log(CurrentMovementState);
            switch (CurrentMovementState)
            {
                case MovementState.Move:
                    {
                        if (CurrentMovementDirection == MovementDirection.Right)
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
                                CurrentMovementState = MovementState.Idle;
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
                            CurrentMovementDirection = MovementDirection.Right;
                            actor.velocity = Vector2.right * MovementSpeed + new Vector2(0, actor.velocity.y);
                        }
                        else
                        {
                            CurrentMovementDirection = MovementDirection.Left;
                            actor.velocity = Vector2.left * MovementSpeed + new Vector2(0, actor.velocity.y);
                        }
                        break;
                    }
                case MovementState.Idle:
                    {
                        if (IdleCounter > 150)
                        {
                            SitCounter = Random.Range(100, 180);
                            CurrentMovementState = MovementState.Sit;
                        }
                        else
                        {
                            IdleCounter--;
                            if (IdleCounter < 0)
                            {
                                CurrentMovementState = MovementState.Decide;
                            }
                        }
                        break;
                    }
                case MovementState.Sit:
                    {
                        SitCounter--;
                        if (SitCounter < 0)
                        {
                            CurrentMovementState = MovementState.Decide;
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
            }
        }
    }
    void SetMovementState()
    {
        Player = PlayerInPercetpionRadius();
        if (Player != null && RangedAttackOnCooldown == false)
            CurrentMovementState = MovementState.RangedAttack;
        else if(RangedAttackActive == false)
        {
            ObjectToChase = PlayerInSight(); //attack einplanen
            if (ObjectToChase != null)
                CurrentMovementState = MovementState.Chase;
            else if (CheckGroundAhead() && CurrentMovementState != MovementState.Idle && CurrentMovementState != MovementState.Sit)
                CurrentMovementState = MovementState.Move;
            else if (CurrentMovementState != MovementState.Idle && CurrentMovementState != MovementState.Sit)
            {
                ChangeDirection();
                CurrentMovementState = MovementState.Move;
            }
        }
    }

    IEnumerator RangedAttack()
    {
        if (Player.position.x > transform.position.x)
            CurrentMovementDirection = MovementDirection.Right;
        else
            CurrentMovementDirection = MovementDirection.Left;
        RangedAttackActive = true;
        GetComponent<Animator>().SetTrigger("RangedAttack");
        RangedAttackOnCooldown = true;
        yield return new WaitForSeconds(2f);
        RangedAttackOnCooldown = false;
    }

    void ShootProjectile() //sollte ein start transform (mundposition bekommen)
    {
        if (CurrentMovementDirection == MovementDirection.Left)
        {
            GameObject projectile = Instantiate(Projectile, LeftStartPos.position, LeftStartPos.rotation);
            projectile.GetComponent<Actor2D>().velocity = Vector2.left * ProjectileSpeed;
        }
        else
        {
            GameObject projectile = Instantiate(Projectile, RightStartPos.position, RightStartPos.rotation);
            projectile.GetComponent<Actor2D>().velocity = Vector2.right * ProjectileSpeed;
        }
        RangedAttackActive = false;
    }


    Transform PlayerInPercetpionRadius()
    {
        Collider2D[] ColliderInRange = Physics2D.OverlapCircleAll(transform.position, PerceptionRadius);
        for (int i = 0; i < ColliderInRange.Length; i++)
        {
            if (ColliderInRange[i].CompareTag("Player"))
            {
                if (CurrentMovementDirection == MovementDirection.Left && transform.position.x > ColliderInRange[i].transform.position.x)
                {
                    float RayCastLenght = Vector2.Distance(transform.position, ColliderInRange[i].transform.position);
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, (ColliderInRange[i].transform.position - transform.position), RayCastLenght, SightBlockingLayers);
                    if (hit == false)
                        return ColliderInRange[i].transform;
                }
                if (CurrentMovementDirection == MovementDirection.Right && transform.position.x < ColliderInRange[i].transform.position.x)
                {
                    float RayCastLenght = Vector2.Distance(transform.position, ColliderInRange[i].transform.position);
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, (ColliderInRange[i].transform.position - transform.position), RayCastLenght, SightBlockingLayers);
                    if (hit == false)
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
        if (CurrentMovementDirection == MovementDirection.Left)
            CurrentMovementDirection = MovementDirection.Right;
        else
            CurrentMovementDirection = MovementDirection.Left;
        DirectionCounter = 150 + Random.Range(0, 150);
    }

    bool CheckGroundAhead()
    {
        RaycastHit2D hit;
        if (CurrentMovementDirection == MovementDirection.Left)
            hit = Physics2D.Raycast(transform.position + Vector3.left, -Vector2.up, GetComponent<Collider2D>().bounds.extents.y + 0.2f);
        else
            hit = Physics2D.Raycast(transform.position + Vector3.right, -Vector2.up, GetComponent<Collider2D>().bounds.extents.y + 0.2f);
        if (hit.collider != null)
            return true;
        return false;
    }
}
