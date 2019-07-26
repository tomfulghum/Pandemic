using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borb : MonoBehaviour
{
    public enum MovementState { Decide, Move, FlyUp, Nosedive, Chase }
    public enum MovementDirection { None, Left, Right }

    [HideInInspector] public MovementState CurrentMovementState = MovementState.Decide; //vllt am anfang auf decide
    [HideInInspector] public MovementDirection CurrentMovementDirection = MovementDirection.Left;

    public float ChaseRange = 3f;
    public float ConeAngle = 35;
    public float MovementSpeed = 3f;
    public LayerMask SightBlockingLayers;
    int DirectionCounter;
    float flightHeight; //bei knockback zurück auf die flight height


    Transform ObjectToChase;
    Actor2D actor;
    Enemy enemy;
    // Start is called before the first frame update
    void Start()
    {
        flightHeight = transform.position.y;
        actor = GetComponent<Actor2D>();
        enemy = GetComponent<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.CurrentEnemyState == Enemy.EnemyState.Moving)
        {
            SetMovementState();
            VisualizeChaseCone();

            switch (CurrentMovementState)
            {
                case MovementState.Move:
                    {
                        CheckFlightHeight();
                        DirectionCounter--;
                        if (DirectionCounter < 0)
                            ChangeDirection();
                        FlyInDirection();
                        break;
                    }
                case MovementState.Chase:
                    {
                        if (ObjectToChase.position.x < transform.position.x)
                            CurrentMovementDirection = MovementDirection.Left;
                        else
                            CurrentMovementDirection = MovementDirection.Right;
                        FlyInDirection();
                        break;
                    }
            }
        }
    }

    void VisualizeChaseCone()
    {
        Vector2 DirectionLine = Vector2.down * GetDistanceToGround();
        Vector2 LeftArc = RotateVector(DirectionLine, ConeAngle);
        Vector2 RightArc = RotateVector(DirectionLine, -ConeAngle);

        Debug.DrawLine(transform.position, (Vector2)transform.position + DirectionLine);
        Debug.DrawLine(transform.position, (Vector2)transform.position + LeftArc);
        Debug.DrawLine(transform.position, (Vector2)transform.position + RightArc);
    }

    float GetDistanceToGround()
    {
        float Distance = -1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, SightBlockingLayers); //evtl eigene layermask //hier nur für ground layer benötigt
        if (hit.collider != null)
            Distance = hit.distance;
        return Distance;
    }

    void FlyInDirection()
    {
        if (CurrentMovementDirection == MovementDirection.Right)
            actor.velocity = Vector2.right * MovementSpeed;
        else
            actor.velocity = Vector2.left * MovementSpeed;
    }

    void SetMovementState()
    {
        ObjectToChase = PlayerInSight();
        if (ObjectToChase != null && ChasePlayer())
            CurrentMovementState = MovementState.Chase;
        else
            CurrentMovementState = MovementState.Move;
    }

    bool ChasePlayer()
    {
        if (Mathf.Abs(ObjectToChase.position.x - transform.position.x) < ChaseRange)
            return true;
        return false;
    }

    Transform PlayerInSight()
    {
        Collider2D[] ColliderInRange = Physics2D.OverlapCircleAll(transform.position, 10); //später evtl auch besser machen GetDistanceToGround()
        for (int i = 0; i < ColliderInRange.Length; i++)
        {
            if (ColliderInRange[i].CompareTag("Player"))
            {
                float RayCastLenght = Vector2.Distance(transform.position, ColliderInRange[i].transform.position);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, (ColliderInRange[i].transform.position - transform.position), RayCastLenght, SightBlockingLayers); //hier auch sight block?
                Vector2 BorbToPlayer = (ColliderInRange[i].transform.position - transform.position).normalized;
                float AngleInDeg = Vector2.Angle(BorbToPlayer, Vector2.down);
                if (hit == false && AngleInDeg < ConeAngle)
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
        DirectionCounter = 150 + Random.Range(0, 100);
    }

    void CheckFlightHeight() //später noch besser machen
    {
        if (transform.position.y != flightHeight)
        {
            if (transform.position.y < flightHeight)
                transform.position = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);
            else
                transform.position = new Vector3(transform.position.x, transform.position.y - 0.05f, transform.position.z);

            if (Mathf.Abs(transform.position.y - flightHeight) < 0.1f)
                transform.position = new Vector3(transform.position.x, flightHeight, transform.position.z);
        }
    }

    Vector2 RotateVector(Vector2 v, float degrees)
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
