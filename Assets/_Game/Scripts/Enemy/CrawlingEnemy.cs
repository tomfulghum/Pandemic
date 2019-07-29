using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//vllt ausrechnen wo er mit einem potenziellen jump landen würde und dann mit höherer chance springen (wenn er auf einer plattform aufkommen würde)
//requires component enemy
//jump intelligenter machen --> evtl auch absprungwinkel ausrechnen
public class CrawlingEnemy : MonoBehaviour
{
    public enum MovementState { Decide, Move, Jump, Falling, Chase } //in air in falling umändern --> wenn noch ground below --> nichts tun nur gravity applyn
    public enum MovementDirection { None, Left, Right } //brauch ich none überhaupt?

    [HideInInspector] public MovementState CurrentMovementState = MovementState.Decide; //vllt am anfang auf decide
    [HideInInspector] public MovementDirection CurrentMovementDirection = MovementDirection.None;

    public float Gravity = 10f;
    public float MovementSpeed = 1f;
    public float ChaseRadius = 3f;
    public bool UseIntelligentJump = true; // default false? //variable jumpprobability --> if 0 then no jump
    public bool UseJump = true;
    //ändern in eine intelligenz skala von 1 - 10 oder so
    public LayerMask SightBlockingLayers;
    int DirectionCounter;

    Transform ObjectToChase;
    Vector2 CurrentVelocity;
    Vector2 JumpDirection;

    Actor2D actor;

    [HideInInspector] public bool Jumping; //nur für animation aktuell --> später verbessern

   //public GameObject DotPrefab;
   //GameObject DotParent; //only for visuals
   //was passiert wenn du den gegner in der luft triffst?
   // Start is called before the first frame update
   //jump values noch anpassen
   void Start()
    {
        actor = GetComponent<Actor2D>();
        //DotParent = new GameObject("Parent Dot Enemy"); //only for visuals
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<Enemy>().CurrentEnemyState == Enemy.EnemyState.Moving) //GetComponent<Enemy>().CurrentEnemyState != Enemy.EnemyState.Dead
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
        //jump direction auf default stellen? --> brauchts das überhaupt?
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
            if ((rnd > 0.9f || (UseIntelligentJump && CheckIfAnyJumpPossible())) && UseJump) //rnd > 0.9f || //--> for better testing without random
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
                    CurrentVelocity = Jump(JumpDirection);
                    Jumping = true;
                    DirectionCounter = 200 + Random.Range(0, 200); //vllt unnötig? oder besser wo anders?
                    CurrentMovementState = MovementState.Decide; //Falling
                    break;
                }
            case MovementState.Falling:
                {
                    //vllt hier velocity nochmal setzen (vector2.x = 0 if movementdirection = none)
                    //gegner bewegt sich mit seiner velcoity aus move weiter --> irgendwas dagegen tun
                    if (actor.collision.below)
                    {
                        Jumping = false;
                        CurrentMovementState = MovementState.Decide;
                    }
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
    Vector2 Jump(Vector2 _JumpDirection)
    {
        return new Vector2(_JumpDirection.x, _JumpDirection.y) * 10; //10 = jumpforce --> variable erstellen //vllt siehts besser aus wenn er seine aktuelle velocity behält?
    }

    bool CheckGroundAhead() //if yes --> decide jump or not //layermask? doesnt hit background?
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

    bool CheckIfAnyJumpPossible() //denke es muss nur noch bisschen an den zahlen geshraubt werden --> was ist mit vector normalisieren
    {
        bool JumpPossible = false;

        if (CurrentMovementDirection == MovementDirection.Right)
        {
            if (CheckJumpPath(transform.position, new Vector2(Mathf.Cos(60 * Mathf.Deg2Rad), Mathf.Sin(60 * Mathf.Deg2Rad)).normalized * 10, 10))
            {
                JumpPossible = true;
                JumpDirection = new Vector2(Mathf.Cos(60 * Mathf.Deg2Rad), Mathf.Sin(60 * Mathf.Deg2Rad)).normalized;
            }
            if (CheckJumpPath(transform.position, new Vector2(Mathf.Cos(45 * Mathf.Deg2Rad), Mathf.Sin(45 * Mathf.Deg2Rad)).normalized * 10, 10))
            {
                JumpPossible = true;
                JumpDirection = new Vector2(Mathf.Cos(45 * Mathf.Deg2Rad), Mathf.Sin(45 * Mathf.Deg2Rad)).normalized;
            }
            if (CheckJumpPath(transform.position, new Vector2(Mathf.Cos(75 * Mathf.Deg2Rad), Mathf.Sin(75 * Mathf.Deg2Rad)).normalized * 10, 10))
            {
                JumpPossible = true;
                JumpDirection = new Vector2(Mathf.Cos(75 * Mathf.Deg2Rad), Mathf.Sin(75 * Mathf.Deg2Rad)).normalized;
            }
        }
        if (CurrentMovementDirection == MovementDirection.Left)
        {
            if (CheckJumpPath(transform.position, new Vector2(-Mathf.Cos(60 * Mathf.Deg2Rad), Mathf.Sin(60 * Mathf.Deg2Rad)).normalized * 10, 10))
            {
                JumpPossible = true;
                JumpDirection = new Vector2(-Mathf.Cos(60 * Mathf.Deg2Rad), Mathf.Sin(60 * Mathf.Deg2Rad)).normalized;
            }
            if (CheckJumpPath(transform.position, new Vector2(-Mathf.Cos(45 * Mathf.Deg2Rad), Mathf.Sin(45 * Mathf.Deg2Rad)).normalized * 10, 10))
            {
                JumpPossible = true;
                JumpDirection = new Vector2(-Mathf.Cos(45 * Mathf.Deg2Rad), Mathf.Sin(45 * Mathf.Deg2Rad)).normalized;
            }
            if (CheckJumpPath(transform.position, new Vector2(-Mathf.Cos(75 * Mathf.Deg2Rad), Mathf.Sin(75 * Mathf.Deg2Rad)).normalized * 10, 10))
            {
                JumpPossible = true;
                JumpDirection = new Vector2(-Mathf.Cos(75 * Mathf.Deg2Rad), Mathf.Sin(75 * Mathf.Deg2Rad)).normalized;
            }
        }
        return JumpPossible;
    }

    bool CheckJumpPath(Vector2 _startPosition, Vector2 _launchVelocity, float _gravity)
    {
        //DotParent.transform.position = _startPosition;
        float TimeBetweenDots = 0.08f; //dafür variable im editor erstellen
        int NumOfChecks = 0;
        bool HitSmth = false;
        float ThrowTime = 0f;
        while (HitSmth == false && NumOfChecks < 50) //30 = max num of checks
        {
            NumOfChecks++;
            Vector2 StartPosition = CalculatePosition(ThrowTime, _launchVelocity, _startPosition, new Vector2(0, -_gravity));
            ThrowTime += TimeBetweenDots;
            Vector2 TargetPosition = CalculatePosition(ThrowTime, _launchVelocity, _startPosition, new Vector2(0, -_gravity));
            float RaycastLenght = (TargetPosition - StartPosition).magnitude;
            RaycastHit2D hit = Physics2D.Raycast(StartPosition, (TargetPosition - StartPosition), RaycastLenght, SightBlockingLayers); //anstatt sightblocking vllt movementblocking nehmen
            if (hit.collider != null && hit.collider.transform.position.y <= hit.point.y) //position compare
                HitSmth = true;
            else if (hit.collider != null && hit.collider.transform.position.y > hit.point.y)
                return false;
            /*
            if (hit.collider == null)
            {
                GameObject trajectoryDot = Instantiate(DotPrefab);
                trajectoryDot.transform.SetParent(DotParent.transform);
                trajectoryDot.transform.position = StartPosition;
            }
            */
        }
        return HitSmth;
    }

    Vector2 CalculatePosition(float elapsedTime, Vector2 _launchVelocity, Vector2 _initialPosition, Vector2 _gravity)
    {
        return _gravity * elapsedTime * elapsedTime * 0.5f + _launchVelocity * elapsedTime + _initialPosition;
    }
}
