using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public bool AllowMovingWhileAttacking;
    public float AttackRange;
    public float AttackAngle;
    public float smashSpeed;
    public LayerMask layerMask;
    public float ControllerTolerance;
    bool Attacking;
    [HideInInspector] public bool Smashing;
    bool FacingLeft;
   // bool HitEnemy; //besser in enemy weil es für jeden enemy einzeln gelten muss
    Vector3 lastPosition;
    Vector2 attackDirection;
    List<Collider2D> enemiesHit;
    float xAxis;
    // Start is called before the first frame update
    void Start()
    {
        enemiesHit = new List<Collider2D>();
        lastPosition = transform.position;
        xAxis = Input.GetAxis("Horizontal");
    }

    // Update is called once per frame
    void Update() //evlt switch case für attack einbauen --> man kann nicht gleichzeitig meteor smash machen und attacken
    {
        if (Input.GetButtonDown("Fire1") && Attacking == false && GetComponent<PlayerHook>().HookActive == false && !Smashing)
        {
            StartCoroutine(AttackSequence());
        }
        if(Attacking)
        {
            VisualizeAttack(attackDirection);
            foreach(Collider2D enemy in CheckEnemyHit(attackDirection))
            {
                if(!enemiesHit.Contains(enemy))
                {
                    enemiesHit.Add(enemy);
                }
            }
        } else
        {
            SetFacingDirection();
            enemiesHit.Clear();
            //attackDirection = Vector2.zero; //nötig?
        }
        if (!GetComponent<Actor2D>().collision.below && Input.GetAxis("MeteorSmash") > ControllerTolerance) //verhindern das man während einem smash hooked
        {
            if(!Smashing && !Attacking)
            {
                StartMeteorSmash();
            }
            //Debug.Log("hello there");
        }
        if(Smashing)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, 1, layerMask); 
            if (hit.collider != null)
            {
                StopMeteorSmash();
                if(hit.collider.CompareTag("BigEnemy") || hit.collider.CompareTag("Enemy"))
                {
                    if (transform.position.x < hit.collider.transform.position.x)
                    {
                        hit.collider.GetComponent<Enemy>().GetHit(false);
                    }
                    else
                    {
                        hit.collider.GetComponent<Enemy>().GetHit(true);
                    }
                    //Debug.Log("i hit an enemy");
                }
            }
            if(GetComponent<Actor2D>().collision.below)
            {
                StopMeteorSmash();
            }
        }
        lastPosition = transform.position;
    }

    void SetFacingDirection()
    {
        float CurrentJoystickDirection = Input.GetAxis("Horizontal");
        if(CurrentJoystickDirection != xAxis)
        {
            if(CurrentJoystickDirection < 0)
            {
                FacingLeft = true;
            }
            else if(CurrentJoystickDirection > 0)
            {
                FacingLeft = false;
            }
            xAxis = CurrentJoystickDirection;
        }
        /*
        if(lastPosition == transform.position)
        {
            return;
        }
        if(lastPosition.x < transform.position.x)
        {
            FacingLeft = false;
        } else
        {
            FacingLeft = true;
        }
        */
    }

    List<Collider2D> CheckEnemyHit(Vector2 _direction) //return list with all enemies hit?
    {
        Collider2D[] ColliderInRange = Physics2D.OverlapCircleAll(transform.position, AttackRange);
        List<Collider2D> EnemiesHit = new List<Collider2D>();
        for(int i = 0; i < ColliderInRange.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (ColliderInRange[i].transform.position - transform.position), AttackRange, layerMask);
            if(hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                Vector2 PlayerToCollider = (ColliderInRange[i].transform.position - transform.position).normalized;
                /*
                Vector2 Direction;
                if (FacingLeft)
                {
                    Direction = Vector2.left;
                }
                else
                {
                    Direction = Vector2.right;
                }
                */
                Vector2 Direction = _direction.normalized;
                float angleInDeg = Vector2.Angle(PlayerToCollider, Direction);
                if(angleInDeg < AttackAngle)
                {
                    if (hit.collider.GetComponent<Enemy>().CurrentlyHit == false)
                    {
                        if (transform.position.x < hit.collider.transform.position.x)
                        {
                            hit.collider.GetComponent<Enemy>().GetHit(false);
                        }
                        else
                        {
                            hit.collider.GetComponent<Enemy>().GetHit(true);
                        }
                        EnemiesHit.Add(hit.collider);
                    }
                }
            }
        }
        return EnemiesHit;
    }

    Vector2 GetAttackDirection(float xInput, float yInput) //evlt auf 8 directions erweitern --> besseren weg dafür finden
    {
        Vector2 Direction;
        if(FacingLeft)
        {
            Direction = Vector2.left;
        }
        else
        {
            Direction = Vector2.right;
        }
        if(Input.GetAxis("Vertical") < -ControllerTolerance)
        {
            Direction = Vector2.down;
        }
        if (Input.GetAxis("Vertical") > ControllerTolerance)
        {
            Direction = Vector2.up;
        }
        if (Input.GetAxis("Horizontal") < -ControllerTolerance)
        {
            Direction = Vector2.left;
        }
        if (Input.GetAxis("Horizontal") > ControllerTolerance)
        {
            Direction = Vector2.right;
        }
        return Direction;
    }
    IEnumerator AttackSequence() //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        Attacking = true;
        attackDirection = GetAttackDirection(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (!GetComponent<Actor2D>().collision.below)
        {
            //Debug.Log("in air");
        }
        if(AllowMovingWhileAttacking || !GetComponent<Actor2D>().collision.below)
        {
            Vector3 currentVelocity = GetComponent<Actor2D>().velocity.normalized * 4;
            GetComponent<PlayerMovement>().DisableUserInput(true);
            GetComponent<PlayerMovement>().SetExternalVelocity(currentVelocity);
        }
        else
        {
            GetComponent<PlayerMovement>().DisableUserInput(true);
        }
        //yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FirstAttack());
        GetComponent<PlayerMovement>().DisableUserInput(false);
        Attacking = false;
    }


    IEnumerator FirstAttack() //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        yield return new WaitForSeconds(0.1f);
        foreach(Collider2D enemy in enemiesHit)
        {
            enemy.GetComponent<Enemy>().CurrentlyHit = false;
        }
        yield return StartCoroutine(SecondAttack()); 
    }

    IEnumerator SecondAttack() //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        attackDirection = RotateVector(attackDirection, Random.Range(10, 15));
        yield return new WaitForSeconds(0.3f);
        foreach (Collider2D enemy in enemiesHit)
        {
            enemy.GetComponent<Enemy>().CurrentlyHit = false;
        }
        yield return StartCoroutine(ThirdAttack());
    }
    IEnumerator ThirdAttack() //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        attackDirection = RotateVector(attackDirection, Random.Range(-15, -20));
        yield return new WaitForSeconds(0.2f);
        foreach (Collider2D enemy in enemiesHit)
        {
            enemy.GetComponent<Enemy>().CurrentlyHit = false;
        }
    }

    void VisualizeAttack(Vector2 _direction)
    {
        //Vector2 DirectionLine;
        /*
        if (FacingLeft)
        {
            DirectionLine = Vector2.left * AttackRange;
        }
        else
        {
            DirectionLine = Vector2.right * AttackRange;
        }
        */
        Vector2 DirectionLine = _direction.normalized * AttackRange;
        Vector2 LeftArc = RotateVector(DirectionLine, AttackAngle);
        Vector2 RightArc = RotateVector(DirectionLine, -AttackAngle);

        Debug.DrawLine(transform.position, (Vector2)transform.position + DirectionLine, Color.green);
        Debug.DrawLine(transform.position, (Vector2)transform.position + LeftArc, Color.green);
        Debug.DrawLine(transform.position, (Vector2)transform.position + RightArc, Color.green);
    }

    void StartMeteorSmash() //rename
    {
        Smashing = true;
        Vector2 VelocityDown = Vector2.down * smashSpeed;
        GetComponent<PlayerMovement>().DisableUserInput(true);
        GetComponent<PlayerMovement>().SetExternalVelocity(VelocityDown);
    }
    void StopMeteorSmash()
    {
        GetComponent<PlayerMovement>().DisableUserInput(false);
        Smashing = false;
        foreach (Collider2D enemy in enemiesHit)
        {
            enemy.GetComponent<Enemy>().CurrentlyHit = false;
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