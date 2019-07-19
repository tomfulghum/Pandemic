using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//requires component enemy
public class CrawlingEnemy : MonoBehaviour
{
    public enum MovementState { Decide, Move, Jump, Falling, Chase } //in air in falling umändern --> wenn noch ground below --> nichts tun nur gravity applyn
    public enum MovementDirection { None, Left, Right } //brauch ich none überhaupt?

    public MovementState CurrentMovementState = MovementState.Decide; //vllt am anfang auf decide
    public MovementDirection CurrentMovementDirection = MovementDirection.None;

    public float Gravity = 10f;
    public float MovementSpeed = 1f;
    public float ChaseRadius = 3f;
    public LayerMask SightBlockingLayers;
    int DirectionCounter;

    Transform ObjectToChase;
    Vector2 CurrentVelocity;

    Actor2D actor;
    //was passiert wenn du den gegner in der luft triffst?
    // Start is called before the first frame update
    //jump values noch anpassen
    void Start()
    {
        actor = GetComponent<Actor2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<Enemy>().CurrentEnemyState == Enemy.EnemyState.Moving)
        {
            if (CurrentMovementState == MovementState.Decide)// && CurrentMovementState != MovementState.Falling)
                SetNextMove();
            if (CurrentMovementState != MovementState.Decide)
                SetMovementPattern();
            Movement();
        }
    }

    void SetNextMove()
    {
        ObjectToChase = PlayerInSight();
        if (ObjectToChase != null)
            CurrentMovementState = MovementState.Chase;
        else if (!GroundBelow())
            CurrentMovementState = MovementState.Falling;
        else if (CheckGroundAhead())
            CurrentMovementState = MovementState.Move;
        else if (CheckGroundAhead() == false)
        {
            float rnd = Random.Range(0f, 1f);
            if (rnd > 0.9f)
                CurrentMovementState = MovementState.Jump;
            else
            {
                ChangeDirection();
                CurrentMovementState = MovementState.Move;
            }
        }
    }


    void SetMovementPattern()
    {
        switch (CurrentMovementState)
        {
            case MovementState.Chase:
                {
                    if (ObjectToChase.position.x > transform.position.x)
                    {
                        CurrentMovementDirection = MovementDirection.Right;
                        CurrentVelocity = Vector2.right * MovementSpeed + new Vector2(0, CurrentVelocity.y);
                    }
                    else
                    {
                        CurrentMovementDirection = MovementDirection.Left;
                        CurrentVelocity = Vector2.left * MovementSpeed + new Vector2(0, CurrentVelocity.y);
                    }
                    CurrentMovementState = MovementState.Decide;
                    break;
                }

            case MovementState.Move:
                {
                    DirectionCounter--;
                    if (DirectionCounter < 0 || actor.collision.left || actor.collision.right)
                        ChangeDirection();
                    if (CurrentMovementDirection == MovementDirection.Right)
                        CurrentVelocity = Vector2.right * MovementSpeed + new Vector2(0, CurrentVelocity.y);
                    else
                        CurrentVelocity = Vector2.left * MovementSpeed + new Vector2(0, CurrentVelocity.y);
                    CurrentMovementState = MovementState.Decide;
                    break;
                }
            case MovementState.Jump:
                {
                    Jump();
                    CurrentMovementState = MovementState.Decide; //Falling
                    break;
                }
            case MovementState.Falling:
                {
                   // if (actor.collision.below)
                        CurrentMovementState = MovementState.Decide;
                    break;
                }
        }
    }

    void ChangeDirection()
    {
        if (CurrentMovementDirection == MovementDirection.Left)
            CurrentMovementDirection = MovementDirection.Right;
        else
            CurrentMovementDirection = MovementDirection.Left;
        DirectionCounter = 200 + Random.Range(0, 200);
    }

    void Movement()
    {
        ApplyGravity();
        CheckCollissions();
        actor.velocity = CurrentVelocity;
    }

    void CheckCollissions()
    {
        if (actor.collision.above || actor.collision.below)
            actor.velocity = new Vector2(CurrentVelocity.x, 0);
        if (actor.collision.left || actor.collision.right)
            actor.velocity = new Vector2(0, CurrentVelocity.y);
    }

    void ApplyGravity()
    {
        CurrentVelocity += Vector2.up * (-10 * Time.deltaTime);
        CurrentVelocity = new Vector2(CurrentVelocity.x, Mathf.Clamp(CurrentVelocity.y, -Gravity, float.MaxValue));
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

    bool GroundBelow()
    {
        if (actor.collision.below)
            return true;
        return false;
    }
    void Jump()
    {
        if (CurrentMovementDirection == MovementDirection.Left)
            CurrentVelocity = Vector2.left * MovementSpeed + new Vector2(-0.5f, 0.5f) * 10; //JumpForce
        else
            CurrentVelocity = Vector2.right * MovementSpeed + new Vector2(0.5f, 0.5f) * 10;
    }

    bool CheckGroundAhead() //if yes --> decide jump or not
    {
        RaycastHit2D hit;
        if (CurrentMovementDirection == MovementDirection.Left)
            hit = Physics2D.Raycast(transform.position + Vector3.left, -Vector2.up, 1);
        else
            hit = Physics2D.Raycast(transform.position + Vector3.right, -Vector2.up, 1);
        if (hit.collider != null)
            return true;
        return false;
    }
}
