using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borb : MonoBehaviour
{
    public enum MovementState { Decide, Move, FlyUp, Nosedive, Dazed, Chase }
    public enum MovementDirection { None, Left, Right }

    [HideInInspector] public MovementState CurrentMovementState = MovementState.Decide; //vllt am anfang auf decide
    [HideInInspector] public MovementDirection CurrentMovementDirection = MovementDirection.Left;

    public float ChaseRange = 3f; //evlt reicht coneangle
    public float ConeAngle = 35;
    public float MovementSpeed = 3f;
    public float DiveSpeed = 20f;
    public float StunTime = 2f;
    public float FlightHeight = 8f;
    public bool UseHeightAdjustments;
    float currentStunTime;
    public LayerMask SightBlockingLayers;

    int DirectionCounter;
    float flightHeight; //bei knockback zurück auf die flight height //--> change in height above ground

    float DiveTriggerRange = 0.2f;

    Transform ObjectToChase;
    Actor2D actor;
    Enemy enemy;
    // Start is called before the first frame update
    void Start()
    {
        currentStunTime = StunTime;
        flightHeight = transform.position.y;
        SetFlightHeight();
        actor = GetComponent<Actor2D>();
        enemy = GetComponent<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.currentEnemyState == Enemy.EnemyState.Moving)
        {
            SetMovementState();
            VisualizeChaseCone();
            if (UseHeightAdjustments)
                SetFlightHeight();

            switch (CurrentMovementState)
            {
                case MovementState.Move: //irgendwo evtl noch des stuck einbauen
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
                case MovementState.Nosedive:
                    {
                        if (CheckGroundHit() == false)
                            actor.velocity = Vector2.down * DiveSpeed;
                        else if (CheckPlayerHit())
                        {
                            CurrentMovementState = MovementState.FlyUp;
                        }
                        else
                        {
                            currentStunTime = StunTime;
                            CurrentMovementState = MovementState.Dazed;
                        }
                        //CurrentMovementState = MovementState.FlyUp;
                        break;
                    }
                case MovementState.Dazed:
                    {
                        currentStunTime -= Time.deltaTime;
                        if (currentStunTime < 0)
                            CurrentMovementState = MovementState.FlyUp;
                        break;
                    }
            }
        }
    }

    void SetFlightHeight()
    {
        float DistanceToGround = GetDistanceToGround(new Vector2(transform.position.x, flightHeight));
        if (DistanceToGround != FlightHeight)
        {
            float HeightDifference = Mathf.Abs(DistanceToGround - FlightHeight);
            if (DistanceToGround <= FlightHeight)
                flightHeight += HeightDifference;
            else
                flightHeight -= HeightDifference;
        }
    }

    void VisualizeChaseCone()
    {
        Vector2 DirectionLine = Vector2.down * GetDistanceToGround(transform.position);
        Vector2 LeftArc = RotateVector(DirectionLine, ConeAngle);
        Vector2 RightArc = RotateVector(DirectionLine, -ConeAngle);

        Debug.DrawLine(transform.position, (Vector2)transform.position + DirectionLine);
        Debug.DrawLine(transform.position, (Vector2)transform.position + LeftArc);
        Debug.DrawLine(transform.position, (Vector2)transform.position + RightArc);
    }

    bool CheckGroundHit()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, DiveSpeed * Time.deltaTime + GetComponent<Collider2D>().bounds.extents.y, SightBlockingLayers); //evlt anstatt 1 die distanz berechnen die er in dem frame zurückgelegt hat //divespeed * Time.deltatime //oder evtl if distance to ground <= 0.1f / 0 oder so
        if (hit.collider != null)
            return true;
        return false;
    }


    bool CheckPlayerHit() //besser wäre wenn es mit raycasts funktioniert oder durch eine funktion in enemy
    {
        Vector2 ColliderBox = new Vector2(GetComponent<BoxCollider2D>().size.x * transform.localScale.x, GetComponent<BoxCollider2D>().size.y * transform.localScale.y);
        Collider2D[] col = Physics2D.OverlapBoxAll(transform.position, ColliderBox, 0); //hitbox anpassen --> evtl etwas größer machen
        foreach (Collider2D collider in col)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;

        /*
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, DiveSpeed * Time.deltaTime + GetComponent<Collider2D>().bounds.extents.y); 
        if (hit.collider != null && hit.collider.CompareTag("Player"))
            return true;
        return false;
        */
    }

    float GetDistanceToGround(Vector3 _position)
    {
        float Distance = -1f;
        RaycastHit2D hit = Physics2D.Raycast(_position, Vector2.down, Mathf.Infinity, SightBlockingLayers); //evtl eigene layermask //hier nur für ground layer benötigt
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
        if (CurrentMovementState != MovementState.Nosedive && CurrentMovementState != MovementState.Dazed) //später ändern
        {
            ObjectToChase = PlayerInSight();
            if (ObjectToChase != null && ChasePlayer() && transform.position.y == flightHeight)
                if (Mathf.Abs(transform.position.x - ObjectToChase.position.x) < DiveTriggerRange)
                    CurrentMovementState = MovementState.Nosedive;
                else
                    CurrentMovementState = MovementState.Chase;
            else
                CurrentMovementState = MovementState.Move;
        }
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
