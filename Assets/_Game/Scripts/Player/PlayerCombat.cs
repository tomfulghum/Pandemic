using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//smash down stärke von der spieler höhe/falllänge abhängig machen
//optional: 8 directional attack input
//dash // 8 direction dash? --> oder wenigstens 6? also nicht nach oben und nicht nach unten?
public class PlayerCombat : MonoBehaviour
{
    //public static bool DisableAllInput = false;
    public bool AllowMovingWhileAttacking; //aktuell noch nicht sogut
    public float AttackRange;
    public float AttackAngle;
    public bool EightDirectionalInput;
    //public int NumberOfAttacks;
    public float smashSpeed;
    public LayerMask layerMask;
    public float ControllerTolerance;

    public float DashSpeed;
    public float DashDuration;
    public float DashCooldown;
    bool Attacking;
    [HideInInspector] public bool Smashing;
    bool FacingLeft;
    bool DashActive;
    bool DashLeft;
   // bool HitEnemy; //besser in enemy weil es für jeden enemy einzeln gelten muss
    Vector3 lastPosition;
    Vector2 attackDirection;
    List<Collider2D> enemiesHit;
    float xAxis;
    Coroutine MovementSlowDown;
    Color originalColor;
    int colorChangeCounter;
    [HideInInspector] public bool CurrentlyHit;
    bool KnockBackActive;
    // Start is called before the first frame update
    void Start()
    {
        originalColor = GetComponent<SpriteRenderer>().color;
        enemiesHit = new List<Collider2D>();
        lastPosition = transform.position;
        xAxis = Input.GetAxis("Horizontal");
    }

    // Update is called once per frame
    void Update() //evlt switch case für attack einbauen --> man kann nicht gleichzeitig meteor smash machen und attacken
    {
        if (PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Waiting || PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Attacking)
        {
            if (Input.GetButtonDown("Dash") && (Input.GetAxis("Horizontal") < ControllerTolerance || Input.GetAxis("Horizontal") > ControllerTolerance) && Attacking == false && !Smashing)
            {
                if (Input.GetAxis("Horizontal") < 0)
                {
                    DashLeft = true;
                }
                else
                {
                    DashLeft = false;
                }
                if (DashActive == false)
                {
                    StartCoroutine(Dash());
                }
            }

            if (Input.GetButtonDown("Fire3") && Attacking == false && !Smashing)
            {
                StartCoroutine(AttackSequence());
            }
            if (Attacking)
            {
                VisualizeAttack(attackDirection);
                foreach (Collider2D enemy in CheckEnemyHit(attackDirection))
                {
                    if (!enemiesHit.Contains(enemy))
                    {
                        enemiesHit.Add(enemy);
                    }
                }
            }
            else
            {
                SetFacingDirection();
                enemiesHit.Clear();
                //attackDirection = Vector2.zero; //nötig?
            }
            if (!GetComponent<Actor2D>().collision.below && Input.GetAxis("MeteorSmash") > ControllerTolerance) //verhindern das man während einem smash hooked
            {
                if (!Smashing && !Attacking)
                {
                    StartMeteorSmash();
                }
                //Debug.Log("hello there");
            }
            if (Smashing)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, 1, layerMask);
                if (hit.collider != null)
                {
                    StopMeteorSmash();
                    if (hit.collider.CompareTag("BigEnemy") || hit.collider.CompareTag("Enemy"))
                    {
                        hit.collider.GetComponent<Enemy>().GetHit(transform, 0.4f);
                        //Debug.Log("i hit an enemy");
                    }
                }
                if (GetComponent<Actor2D>().collision.below && !GetComponent<Actor2D>().collision.below.CompareTag("Enemy"))
                {
                    StopMeteorSmash();
                }
            }
            lastPosition = transform.position;
        } 
        if(KnockBackActive)
        {
            colorChangeCounter++;
            if (colorChangeCounter % 5 == 0)
            {
                GetComponent<SpriteRenderer>().color = Color.white;
            }
            else
            {
                GetComponent<SpriteRenderer>().color = originalColor;
            }
        }
    }

    IEnumerator Dash()
    {
        Vector2 velocity = Vector2.zero;
        DashActive = true;
        if (DashLeft)
        {
            velocity = Vector2.left; // immer auf die timescale achten wegen dem hook / Time.timeScale
        }
        else
        {
            velocity = Vector2.right;
        }
        GetComponent<PlayerMovement>().DisableUserInput(true);
        GetComponent<PlayerMovement>().SetExternalVelocity(velocity*DashSpeed); 
        yield return new WaitForSeconds(DashDuration);
        GetComponent<PlayerMovement>().DisableUserInput(false);
        yield return new WaitForSeconds(DashCooldown);
        DashActive = false;
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
                        hit.collider.GetComponent<Enemy>().GetHit(transform, 0.2f);
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
        if(EightDirectionalInput)
        {
            if(Input.GetAxis("Vertical") < -ControllerTolerance && Input.GetAxis("Horizontal") < -ControllerTolerance)
            {
                Direction = new Vector2(-0.5f, -0.5f).normalized;
            }
            if (Input.GetAxis("Vertical") > ControllerTolerance && Input.GetAxis("Horizontal") > ControllerTolerance)
            {
                Direction = new Vector2(0.5f, 0.5f).normalized;
            }
            if (Input.GetAxis("Vertical") < -ControllerTolerance && Input.GetAxis("Horizontal") > ControllerTolerance)
            {
                Direction = new Vector2(0.5f, -0.5f).normalized;
            }
            if (Input.GetAxis("Vertical") > ControllerTolerance && Input.GetAxis("Horizontal") < -ControllerTolerance)
            {
                Direction = new Vector2(-0.5f, 0.5f).normalized;
            }
        }
        return Direction;
    }

    IEnumerator SlowMovementDown(Vector2 _startvelocity)
    {
       // Debug.Log(_startvelocity.magnitude);
        float MovingSpeed = _startvelocity.magnitude;
        while (_startvelocity.magnitude*MovingSpeed > 0.1f)
        {
            MovingSpeed *= 0.8f;
            Vector2 newVelocity = _startvelocity.normalized * MovingSpeed * 0.99f;
            //Debug.Log(newVelocity.magnitude);
            GetComponent<PlayerMovement>().SetExternalVelocity(newVelocity);
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator AttackSequence() //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        Attacking = true;
        attackDirection = GetAttackDirection(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (GetComponent<Actor2D>().collision.below)
        {
            //Debug.Log("in air");
            GetComponent<PlayerMovement>().DisableUserInput(true);
            Vector3 currentVelocity = GetComponent<Actor2D>().velocity;
            MovementSlowDown = StartCoroutine(SlowMovementDown(currentVelocity));
        }
        /*
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
        */
        //yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FirstAttack());
       // yield return StartCoroutine(Attack(NumberOfAttacks, attackDirection)); //funktioniert noch nicht
        GetComponent<PlayerMovement>().DisableUserInput(false);
        Attacking = false;
        if (MovementSlowDown != null)
        {
            StopCoroutine(MovementSlowDown);
        }
    }

    IEnumerator Attack(int _numOfRepeats, Vector2 _startDirection)
    {
        attackDirection = RotateVector(_startDirection, Random.Range(-15,15));
        Debug.Log("attackDirection: " + attackDirection);
        yield return new WaitForSeconds(0.1f + Random.Range(0.0f, 0.2f)); //vllt ohne random
        foreach (Collider2D enemy in enemiesHit)
        {
            enemy.GetComponent<Enemy>().CurrentlyHit = false;
        }
        if (_numOfRepeats > 0)
        {
            StartCoroutine(Attack(_numOfRepeats - 1, _startDirection));
        }
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
        yield return StartCoroutine(ThirdAttack()); //ThirdAttack()
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
        Debug.Log(enemiesHit.Count);
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

    public void GetHit(Transform _knockBackOrigin, float _strength) //bandaid fix for knockbackdirectino
    {
        StopCoroutine("KnockBack");
        //StopAllCoroutines(); //wirklich alle stoppen? --> wahrscheinlich sinnvoll
        StartCoroutine(KnockBack(10, _knockBackOrigin, _strength));
        CurrentlyHit = true;
    }

    IEnumerator KnockBack(float _repetissions, Transform _knockBackOrigin, float _knockBackStrength) //knock back direction als Parameter übergeben
    {
        KnockBackActive = true;
        PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Disabled;
        GetComponent<PlayerMovement>().DisableUserInput(true);
        for (int i = 0; i < _repetissions; i++)
        {
            float test = 1 - Mathf.Pow((i), 3) / 100;
            if (test < 0)
            {
                test = 0;
            }
            //Debug.Log(test);
            if (_knockBackOrigin.transform.position.x > transform.position.x)
            {
                transform.position = new Vector2(transform.position.x - _knockBackStrength * test, transform.position.y);
            }
            else
            {
                transform.position = new Vector2(transform.position.x + _knockBackStrength * test, transform.position.y);
            }
            yield return new WaitForSeconds(0.03f);
        }
        KnockBackActive = false;
        CurrentlyHit = false;
        GetComponent<SpriteRenderer>().color = originalColor;
        colorChangeCounter = 0;
        PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Waiting;
        GetComponent<PlayerMovement>().DisableUserInput(false);
    }

    //GetHit //Stagger ...
}