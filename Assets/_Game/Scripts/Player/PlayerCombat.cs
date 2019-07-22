using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//smash down stärke von der spieler höhe/falllänge abhängig machen
//melee attack: kein input: stehenbleiben, input: bewegung in input richtung --> am besten wie beim knock back 
//default values
//evtl combo time kürzer machen aber dafür schon kurz vor ende der attack input annehmen
public class PlayerCombat : MonoBehaviour
{
    public enum AttackState { None, Attack, Dash, Smash } //attack state and attack type?
    [HideInInspector] public AttackState CurrentAttackState = AttackState.None;

    public float AttackRange = 2.5f;
    public float SmashSpeed = 20f;
    public LayerMask layerMask;
    public float ControllerTolerance = 0.5f;

    public float DashSpeed = 20f;
    public float DashDuration = 0.2f;
    public float DashCooldown = 0.5f;

    public float TimeToCombo = 0.1f;
    public float MeleeAttackTime = 0.3f;
    public LayerMask GroundCollission;

    float AttackAngle = 25f; //veraltet nur noch füs visuelle da

    bool Attacking;
    bool FacingLeft;
    //bool DashCooldownActive; //oder dash on cooldown
    Vector2 attackDirection;
    List<Collider2D> enemiesHit;
    float xAxis;
    Coroutine MovementSlowDown;
    Color originalColor;
    int colorChangeCounter;

    //Combo Attack Test
    int AttackNumber;
    Coroutine MeleeAttack;
    Coroutine MeleeMovement;
    bool CurrentlyAttacking;
    float CurrentAngle;
    bool AlreadyAttacked;

    Actor2D actor;
    // Start is called before the first frame update
    void Start()
    {
        originalColor = GetComponent<SpriteRenderer>().color;
        enemiesHit = new List<Collider2D>();
        xAxis = Input.GetAxis("Horizontal");
        actor = GetComponent<Actor2D>();
    }

    // Update is called once per frame
    void Update() //evlt switch case für attack einbauen --> man kann nicht gleichzeitig meteor smash machen und attacken
    {
        if (PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Waiting || PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Attacking)
        {
            SetPlayerState();
            SetFacingDirection(); //ist es klug jedes frame zu setzen? --> wenn ja so einbauen das es auch funktioniert 
            if (Input.GetButtonDown("Dash") && CurrentAttackState == AttackState.None) //&& (Input.GetAxis("Horizontal") < ControllerTolerance || Input.GetAxis("Horizontal") > ControllerTolerance)
            {
                StartCoroutine(Dash());
            }

            if ((Input.GetButtonDown("Fire3") || AlreadyAttacked) && (CurrentAttackState == AttackState.None || CurrentAttackState == AttackState.Attack))
            {
                AlreadyAttacked = true;
                if (MeleeAttack == null)
                {
                    attackDirection = GetAttackDirection(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                    MeleeAttack = StartCoroutine(AttackSequence(AttackNumber));
                }
                else if (CurrentlyAttacking == false)
                {
                    StopCoroutine(MeleeAttack);
                    MeleeAttack = StartCoroutine(AttackSequence(AttackNumber));
                }
            }

            if (!GetComponent<Actor2D>().collision.below && Input.GetAxis("MeteorSmash") > ControllerTolerance && CurrentAttackState == AttackState.None)
            {
                StartMeteorSmash();
            }

            /*
            if(CurrentAttackState != AttackState.None)
            {
                switch(CurrentAttackState)
                {
                    case AttackState.Attack:
                        {
                            break;
                        }
                    case AttackState.Dash: //brauch ich da überhaupt was?
                        {
                            break;
                        }
                    case AttackState.Smash:
                        {
                            break;
                        }
                }
            }
            */
            if (CurrentAttackState == AttackState.Attack && CurrentlyAttacking)
            {
                VisualizeAttack(attackDirection);
                foreach (Collider2D enemy in CheckEnemyHit(attackDirection))
                {
                    if (!enemiesHit.Contains(enemy))
                        enemiesHit.Add(enemy);
                }
            }
            else
                //enemiesHit.Clear(); //brauch ich das hier? --> führt dazu das der enemy in meteor smash teilweise nicht resettet wird

            if (CurrentAttackState == AttackState.Smash)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, 1, layerMask);
                if (hit.collider != null)
                {
                    StopMeteorSmash();
                    if (hit.collider.CompareTag("BigEnemy") || hit.collider.CompareTag("Enemy"))
                        hit.collider.GetComponent<Enemy>().GetHit(transform, 10);
                }
                if (GetComponent<Actor2D>().collision.below && !GetComponent<Actor2D>().collision.below.CompareTag("Enemy"))
                    StopMeteorSmash();
            }
        }
        if (PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Disabled)
        {
            colorChangeCounter++;
            if (colorChangeCounter % 5 == 0)
                GetComponent<SpriteRenderer>().color = Color.white;
            else
                GetComponent<SpriteRenderer>().color = originalColor;
        }
    }

    void SetPlayerState() //s. Playerhook
    {
        if (CurrentAttackState != AttackState.None)
            PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Attacking;
        else
            if (PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Attacking && CurrentAttackState == AttackState.None)
            PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Waiting;
    }

    IEnumerator Dash()
    {
        Vector2 velocity = Vector2.zero;
        CurrentAttackState = AttackState.Dash;
        if (FacingLeft)
            velocity = Vector2.left; // immer auf die timescale achten wegen dem hook / Time.timeScale --> kann man überhaupt während einem timeslow dashen?
        else
            velocity = Vector2.right;
        GetComponent<PlayerMovement>().DisableUserInput(true);
        GetComponent<PlayerMovement>().SetExternalVelocity(velocity * DashSpeed);
        yield return new WaitForSeconds(DashDuration);
        GetComponent<PlayerMovement>().DisableUserInput(false);
        yield return new WaitForSeconds(DashCooldown);
        CurrentAttackState = AttackState.None; //würde aktuell bedeuten das man während dem dash cooldown auch nicht anders angreifen kann --> ändern
    }

    void SetFacingDirection()
    {
        float CurrentJoystickDirection = Input.GetAxis("Horizontal");
        if (CurrentJoystickDirection != xAxis)
        {
            if (CurrentJoystickDirection < 0)
                FacingLeft = true;
            else if (CurrentJoystickDirection > 0)
                FacingLeft = false;
            xAxis = CurrentJoystickDirection;
        }
    }

    List<Collider2D> CheckEnemyHit(Vector2 _direction) //return list with all enemies hit?
    {
        Collider2D[] ColliderInRange = Physics2D.OverlapCircleAll(transform.position, AttackRange);
        List<Collider2D> EnemiesHit = new List<Collider2D>();
        for (int i = 0; i < ColliderInRange.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (ColliderInRange[i].transform.position - transform.position), AttackRange, layerMask);
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                Vector2 PlayerToCollider = (ColliderInRange[i].transform.position - transform.position).normalized;
                Vector2 Direction = _direction.normalized;
                float angleInDeg = Vector2.Angle(PlayerToCollider, Direction);
                if (angleInDeg < 90)
                {
                    if (hit.collider.GetComponent<Enemy>().CurrentlyHit == false)
                    {
                        hit.collider.GetComponent<Enemy>().GetHit(transform, 7); //wie bei throwable object umstellen
                        EnemiesHit.Add(hit.collider);
                    }
                }
            }
        }
        return EnemiesHit;
    }

    Vector2 GetAttackDirection(float xInput, float yInput)
    {
        Vector2 Direction;
        if (FacingLeft)
            Direction = Vector2.left;
        else
            Direction = Vector2.right;
        if (Input.GetAxis("Vertical") < -ControllerTolerance)
            Direction = Vector2.down;
        if (Input.GetAxis("Vertical") > ControllerTolerance)
            Direction = Vector2.up;
        if (Input.GetAxis("Horizontal") < -ControllerTolerance)
            Direction = Vector2.left;
        if (Input.GetAxis("Horizontal") > ControllerTolerance)
            Direction = Vector2.right;
        return Direction;
    }

    IEnumerator AttackSequence(int _numOfAttack) //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        CurrentAttackState = AttackState.Attack;
        if (GetComponent<Actor2D>().collision.below || GroundBelow())
        {
            GetComponent<PlayerMovement>().DisableUserInput(true);
        }

        if (_numOfAttack == 0)
            yield return StartCoroutine(FirstAttack());
        if (_numOfAttack == 1)
            yield return StartCoroutine(SecondAttack());
        if (_numOfAttack == 2)
            yield return StartCoroutine(ThirdAttack());

        yield return new WaitForSeconds(TimeToCombo);
        GetComponent<PlayerMovement>().DisableUserInput(false);
        CurrentAttackState = AttackState.None;
        AttackNumber = 0;
        MeleeAttack = null;
    }

    IEnumerator AttackMovement(float _repetissions, Vector2 _direction, float _KnockBackForce) //knock back direction als Parameter übergeben //vllt cancel all movement (hook usw.) einbauen
    {
        for (int i = 0; i < _repetissions; i++)
        {
            float test = 1 - Mathf.Pow((i), 2) / 100;
            if (test < 0)
                test = 0;

            Vector2 MovementDirection = _direction.normalized;
            actor.velocity = MovementDirection * test * _KnockBackForce; //currently no gravity? --> wahrscheinlich ne gute idee //funktioniertt das mit der enemy collission?
            if (actor.collision.above || actor.collision.below)
                actor.velocity = new Vector2(actor.velocity.x, 0);
            if (actor.collision.left || actor.collision.right)
                actor.velocity = new Vector2(0, actor.velocity.y);

            yield return new WaitForSeconds(0.005f);
        }
    }


    bool GroundBelow() //short fix because of actor velocity problem
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1, GroundCollission);
        if (hit.collider != null)
            return true;
        return false;
    }

    void AttackMove()
    {
        if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) //was ist mit down und up?
        {
            if (MeleeAttack != null)
                StopCoroutine(MeleeAttack);
            if (FacingLeft)
                MeleeMovement = StartCoroutine(AttackMovement(20, new Vector2(-1, -1), 7 + 1 / MeleeAttackTime));
            else
                MeleeMovement = StartCoroutine(AttackMovement(20, new Vector2(1, -1), 7 + 1 / MeleeAttackTime));
        }
    }

    IEnumerator FirstAttack() //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        if (actor.collision.below || GroundBelow())
            AttackMove();
        CurrentAngle = AttackAngle;
        AttackNumber = 1;
        CurrentlyAttacking = true;
        AlreadyAttacked = false;
        yield return new WaitForSeconds(MeleeAttackTime);

        foreach (Collider2D enemy in enemiesHit)
            enemy.GetComponent<Enemy>().CurrentlyHit = false;

        CurrentlyAttacking = false;
    }

    IEnumerator SecondAttack() //darauf achten während dem air attack etwas gravity dazuzurechnen --> nicht nötig weil movement in air nicht disabled ist
    {
        if (actor.collision.below || GroundBelow())
            AttackMove();
        CurrentAngle = AttackAngle;
        AttackNumber = 2;
        CurrentlyAttacking = true;
        AlreadyAttacked = false;
        attackDirection = RotateVector(attackDirection, 20);
        yield return new WaitForSeconds(MeleeAttackTime);

        foreach (Collider2D enemy in enemiesHit)
            enemy.GetComponent<Enemy>().CurrentlyHit = false;

        CurrentlyAttacking = false;
    }
    IEnumerator ThirdAttack() //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        if (actor.collision.below || GroundBelow())
            AttackMove();
        CurrentAngle = AttackAngle;
        CurrentlyAttacking = true;
        AlreadyAttacked = false;
        attackDirection = RotateVector(attackDirection, -40);
        yield return new WaitForSeconds(MeleeAttackTime);

        foreach (Collider2D enemy in enemiesHit)
            enemy.GetComponent<Enemy>().CurrentlyHit = false;

        GetComponent<PlayerMovement>().DisableUserInput(false);
        CurrentlyAttacking = false;
        AttackNumber = 0;
        StopCoroutine(MeleeAttack);
        MeleeAttack = null;
        CurrentAttackState = AttackState.None; //function end attack schreiben
    }

    void VisualizeAttack(Vector2 _direction)
    {
        Vector2 DirectionLine = _direction.normalized * AttackRange;
        Vector2 LeftArc = RotateVector(DirectionLine, AttackAngle);
        Vector2 RightArc = RotateVector(DirectionLine, -AttackAngle);

        Vector2 HitVisual = RotateVector(DirectionLine, CurrentAngle);
        //CurrentAngle -= (AttackAngle*2) -  1 / FirstAttackTime; //Time.deltaTime?
        CurrentAngle -= 1 * 1 / MeleeAttackTime;

        Debug.DrawLine(transform.position, (Vector2)transform.position + HitVisual, Color.red);
        Debug.DrawLine(transform.position, (Vector2)transform.position + DirectionLine, Color.green);
        Debug.DrawLine(transform.position, (Vector2)transform.position + LeftArc, Color.green);
        Debug.DrawLine(transform.position, (Vector2)transform.position + RightArc, Color.green);
    }

    void StartMeteorSmash() //rename
    {
        CurrentAttackState = AttackState.Smash;
        Vector2 VelocityDown = Vector2.down * SmashSpeed;
        GetComponent<PlayerMovement>().DisableUserInput(true);
        GetComponent<PlayerMovement>().SetExternalVelocity(VelocityDown);
    }
    void StopMeteorSmash()
    {
        GetComponent<PlayerMovement>().DisableUserInput(false);
        CurrentAttackState = AttackState.None;
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

    public void GetHit(Transform _knockBackOrigin, float _KnockBackForce) //bandaid fix for knockbackdirectino //player knockback noch bisshen stärker einstellen
    {
        //vllt die überprüfung ob der hit gilt hier rein machen --> viel besser
        StopCoroutine("KnockBack"); //sinvoll? oder vllt nur get hit wenn knock back aktuell nicht aktiv ist?
        //was ist mit attacksequence usw.? die auch stoppen?
        //StopAllCoroutines(); //wirklich alle stoppen? --> wahrscheinlich sinnvoll
        StartCoroutine(KnockBack(10, _knockBackOrigin, _KnockBackForce));
    }

    IEnumerator KnockBack(float _repetissions, Transform _knockBackOrigin, float _KnockBackForce) //knock back direction als Parameter übergeben //vllt cancel all movement (hook usw.) einbauen
    {
        PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Disabled;
        GetComponent<PlayerMovement>().DisableUserInput(true);
        for (int i = 0; i < _repetissions; i++)
        {
            float test = 1 - Mathf.Pow((i), 3) / 100;
            if (test < 0)
                test = 0;

            Vector2 KnockBackDirection = (transform.position - _knockBackOrigin.position).normalized;
            actor.velocity = KnockBackDirection * test * _KnockBackForce; //currently no gravity? --> wahrscheinlich ne gute idee //funktioniertt das mit der enemy collission?
            if (actor.collision.above || actor.collision.below)
                actor.velocity = new Vector2(actor.velocity.x, 0);
            if (actor.collision.left || actor.collision.right)
                actor.velocity = new Vector2(0, actor.velocity.y);

            yield return new WaitForSeconds(0.03f);
        }
        GetComponent<SpriteRenderer>().color = originalColor;
        colorChangeCounter = 0;
        PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Waiting;
        GetComponent<PlayerMovement>().DisableUserInput(false);
    }

    //GetHit //Stagger ...
}