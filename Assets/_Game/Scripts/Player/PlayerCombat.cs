using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//smash down stärke von der spieler höhe/falllänge abhängig machen
//melee attack: kein input: stehenbleiben, input: bewegung in input richtung --> am besten wie beim knock back 
//default values
//evtl combo time kürzer machen aber dafür schon kurz vor ende der attack input annehmen
public class PlayerCombat : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    public enum AttackState //attack state and attack type?
    {
        None,
        Attack,
        Dash,
        Smash
    }

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float m_attackRange = 2.5f;
    [SerializeField] private float m_smashSpeed = 20f;
    [SerializeField] private LayerMask m_layerMask = default; //später renamen --> enemy hit mask oder so //ground ist wichtig das man gegner nicht durch wände schlagen kann
    [SerializeField] private float m_controllerTolerance = 0.5f;

    [SerializeField] private float m_dashSpeed = 20f;
    [SerializeField] private float m_dashDuration = 0.2f;
    [SerializeField] private float m_dashCooldown = 0.5f;

    [SerializeField] private float m_timeToCombo = 0.1f;
    [SerializeField] private float m_meleeAttackTime = 0.3f;

    [SerializeField] private float m_hitFreezeTime = 0.5f;
    [SerializeField] private float m_hitKnockbackTime = 0.2f;

    //******************//
    //    Properties    //
    //******************//

    public AttackState currentAttackState
    {
        get { return m_currentAttackState; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private AttackState m_currentAttackState = AttackState.None;

    private float m_attackAngle = 25f; //veraltet nur noch füs visuelle da

    private bool m_attacking;
    private bool m_facingLeft;
    //private bool DashCooldownActive; //oder dash on cooldown
    private Vector2 m_attackDirection;
    private List<Collider2D> m_enemiesHit;
    private float m_xAxis;
    private Coroutine m_movementSlowDown;
    private Color m_originalColor;
    private int m_colorChangeCounter;

    //Combo Attack Test
    private int m_attackNumber;
    private Coroutine m_meleeAttack;
    private Coroutine m_meleeMovement;
    private bool m_currentlyAttacking;
    private float m_currentAngle;
    private bool m_alreadyAttacked;
    private bool m_attackCoolDownActive; //--> evlt später eigenen state für cooldown einbauzne
    private bool m_dashCoolDownActive; //--> irgendwie besser lösen
    private bool m_comboActive;

    private bool m_invincible; //After every attack some invincibility frames --> change later to a priority system
    private float m_invincibilityTime = 0.2f; //time during which the player is invincible --> priority system?, invincibility time evtl per move?

    private int m_currentHitPriority = 1;

    private Coroutine m_knockbackCoroutine = null;

    private PlayerInput m_input;
    private Actor2D m_actor;
    private PlayerMovement m_pm;
    private SpriteRenderer m_spriteRenderer;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    //cooldown on melee attack? --> allgemein nach jedem angriff kurz 0.4f sec oder so wartezeit?
    void Start()
    {
        m_originalColor = GetComponent<SpriteRenderer>().color;
        m_enemiesHit = new List<Collider2D>();
        m_xAxis = Input.GetAxis("Horizontal");

        m_input = GetComponent<PlayerInput>();
        m_actor = GetComponent<Actor2D>();
        m_pm = GetComponent<PlayerMovement>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() //evlt switch case für attack einbauen --> man kann nicht gleichzeitig meteor smash machen und attacken
    {
        if (PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Waiting || PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Attacking) {
            SetPlayerState();
            SetFacingDirection(); //ist es klug jedes frame zu setzen? --> wenn ja so einbauen das es auch funktioniert 
            if (m_input.player.GetButtonDown(m_input.dashButton) && m_currentAttackState == AttackState.None && m_dashCoolDownActive == false) {  //&& (Input.GetAxis("Horizontal") < ControllerTolerance || Input.GetAxis("Horizontal") > ControllerTolerance)
                StartCoroutine(Dash());
            }

            if ((m_input.player.GetButtonDown(m_input.attackButton) || m_alreadyAttacked) && (m_currentAttackState == AttackState.None || m_currentAttackState == AttackState.Attack) && m_attackCoolDownActive == false) {
                if (m_comboActive) {
                    m_alreadyAttacked = true;
                }

                if (m_meleeAttack == null) {
                    m_attackDirection = GetAttackDirection(m_input.player.GetAxis(m_input.aimHorizontalAxis), m_input.player.GetAxis(m_input.aimVerticalAxis));
                    m_meleeAttack = StartCoroutine(AttackSequence(m_attackNumber));
                } else if (m_currentlyAttacking == false) {
                    StopCoroutine(m_meleeAttack);
                    m_meleeAttack = StartCoroutine(AttackSequence(m_attackNumber));
                }
            }

            if (!m_actor.contacts.below && m_input.player.GetAxis(m_input.smashButton) > m_controllerTolerance && m_currentAttackState == AttackState.None) {
                StartMeteorSmash();
            }

            if (m_currentAttackState == AttackState.Attack && m_currentlyAttacking) {
                VisualizeAttack(m_attackDirection);
                foreach (Collider2D enemy in CheckEnemyHit(m_attackDirection)) {
                    if (!m_enemiesHit.Contains(enemy)) {
                        m_enemiesHit.Add(enemy);
                    }
                }
            }

            if (m_currentAttackState == AttackState.Smash) {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, 1, m_layerMask);
                if (hit.collider != null) {
                    if (hit.collider.CompareTag("BigEnemy") || hit.collider.CompareTag("Enemy")) {
                        hit.collider.GetComponent<Enemy>().GetHit(transform, 15, m_currentHitPriority);
                    } else {
                        StopMeteorSmash();
                    }
                }

                if (m_actor.contacts.below && m_actor.contacts.below.CompareTag("Enemy")) { // geht nicht weil actor nicht mit enemy kollidiert
                    m_actor.contacts.below.GetComponent<Enemy>().GetHit(transform, 15, m_currentHitPriority);
                }

                if (m_actor.contacts.below && !m_actor.contacts.below.CompareTag("Enemy")) {
                    StopMeteorSmash();
                }
            }
        }

        if (PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Disabled) {
            m_colorChangeCounter++;
            if (m_colorChangeCounter % 5 == 0) {
                m_spriteRenderer.color = Color.white;
            } else {
                m_spriteRenderer.color = m_originalColor;
            }
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void SetPlayerState() //s. Playerhook
    {
        if (m_currentAttackState != AttackState.None) {
            PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Attacking;
        } else {
            if (PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Attacking && m_currentAttackState == AttackState.None) {
                PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Waiting;
            }
        }
    }

    private IEnumerator Dash()
    {
        m_dashCoolDownActive = true;
        Vector2 velocity = Vector2.zero;
        m_currentAttackState = AttackState.Dash;
        if (m_facingLeft) {
            velocity = Vector2.left; // immer auf die timescale achten wegen dem hook / Time.timeScale --> kann man überhaupt während einem timeslow dashen?
        } else {
            velocity = Vector2.right;
        }

        m_pm.DisableUserInput(true);
        m_pm.externalVelocity = velocity * m_dashSpeed;
        yield return new WaitForSeconds(m_dashDuration);
        m_pm.DisableUserInput(false);
        m_currentAttackState = AttackState.None;
        yield return new WaitForSeconds(m_dashCooldown);
        m_dashCoolDownActive = false;
    }

    private void SetFacingDirection()
    {
        float currentJoystickDirection = m_input.player.GetAxis(m_input.aimHorizontalAxis);
        if (currentJoystickDirection != m_xAxis) {
            if (currentJoystickDirection < 0) {
                m_facingLeft = true;
            } else if (currentJoystickDirection > 0) {
                m_facingLeft = false;
            }
            m_xAxis = currentJoystickDirection;
        }
    }

    private List<Collider2D> CheckEnemyHit(Vector2 _direction) //return list with all enemies hit?
    {
        Collider2D[] colliderInRange = Physics2D.OverlapCircleAll(transform.position, m_attackRange);
        List<Collider2D> enemiesHit = new List<Collider2D>();
        for (int i = 0; i < colliderInRange.Length; i++) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (colliderInRange[i].transform.position - transform.position), m_attackRange, m_layerMask);
            if (hit.collider != null && (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("BigEnemy"))) {
                Vector2 playerToCollider = (colliderInRange[i].transform.position - transform.position).normalized;
                Vector2 direction = _direction.normalized;
                float angleInDeg = Vector2.Angle(playerToCollider, direction);
                if (angleInDeg < 90) {
                    hit.collider.GetComponent<Enemy>().GetHit(transform, 7, m_currentHitPriority); //wie bei throwable object umstellen
                    enemiesHit.Add(hit.collider);
                }
            }
        }
        return enemiesHit;
    }

    private Vector2 GetAttackDirection(float _xInput, float _yInput)
    {
        Vector2 direction;
        if (m_facingLeft) {
            direction = Vector2.left;
        } else {
            direction = Vector2.right;
        }

        if (m_input.player.GetAxis(m_input.aimVerticalAxis) < -m_controllerTolerance) {
            direction = Vector2.down;
        }
        if (m_input.player.GetAxis(m_input.aimVerticalAxis) > m_controllerTolerance) {
            direction = Vector2.up;
        }
        if (m_input.player.GetAxis(m_input.aimHorizontalAxis) < -m_controllerTolerance) {
            direction = Vector2.left;
        }
        if (m_input.player.GetAxis(m_input.aimHorizontalAxis) > m_controllerTolerance) {
            direction = Vector2.right;
        }

        return direction;
    }

    private IEnumerator AttackCooldown() //vllt brauch ich das gar nicht --> testen ob es sich gut anfühlt
    {
        m_attackCoolDownActive = true;
        yield return new WaitForSeconds(0.1f);
        m_attackCoolDownActive = false;
    }

    private IEnumerator AttackSequence(int _numOfAttack) //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        m_comboActive = true;
        m_currentAttackState = AttackState.Attack;
        if (m_actor.contacts.below) {
            m_pm.DisableUserInput(true);
        }

        if (_numOfAttack == 0) {
            yield return StartCoroutine(FirstAttack());
        }
        if (_numOfAttack == 1) {
            yield return StartCoroutine(SecondAttack());
        }
        if (_numOfAttack == 2) {
            yield return StartCoroutine(ThirdAttack());
        }

        yield return new WaitForSeconds(m_timeToCombo);
        m_pm.DisableUserInput(false);
        m_currentAttackState = AttackState.None;
        m_attackNumber = 0;
        m_meleeAttack = null;
        m_comboActive = false;
        StartCoroutine(AttackCooldown());
        m_currentHitPriority = 1;
    }

    private IEnumerator AttackMovement(float _repetitions, Vector2 _direction, float _knockBackForce) //knock back direction als Parameter übergeben //vllt cancel all movement (hook usw.) einbauen
    {
        for (int i = 0; i < _repetitions; i++) {
            float test = 1 - Mathf.Pow((i), 2) / 100;
            if (test < 0)
                test = 0;

            Vector2 MovementDirection = _direction.normalized;
            m_pm.externalVelocity = MovementDirection * test * _knockBackForce; //currently no gravity? --> wahrscheinlich ne gute idee //funktioniertt das mit der enemy collission?
            if (m_actor.contacts.above || m_actor.contacts.below) {
                m_pm.externalVelocity = new Vector2(m_actor.velocity.x, 0);
            }
            if (m_actor.contacts.left || m_actor.contacts.right) {
                m_pm.externalVelocity = new Vector2(0, m_actor.velocity.y);
            }

            yield return new WaitForSeconds(0.005f);
        }
    }

    private void AttackMove()
    {
        if (m_input.player.GetAxis(m_input.aimVerticalAxis) != 0 || m_input.player.GetAxis(m_input.aimHorizontalAxis) != 0) //was ist mit down und up?
        {
            if (m_meleeAttack != null) {
                StopCoroutine(m_meleeAttack);
            }
            if (GetAttackDirection(m_input.player.GetAxis(m_input.aimHorizontalAxis), m_input.player.GetAxis(m_input.aimVerticalAxis)) == Vector2.left) {
                m_meleeMovement = StartCoroutine(AttackMovement(20, new Vector2(-1, -1), 7 + 1 / m_meleeAttackTime));
            } else if ((GetAttackDirection(m_input.player.GetAxis(m_input.aimHorizontalAxis), m_input.player.GetAxis(m_input.aimVerticalAxis)) == Vector2.right)) {
                m_meleeMovement = StartCoroutine(AttackMovement(20, new Vector2(1, -1), 7 + 1 / m_meleeAttackTime));
            }
        }
    }

    private IEnumerator FirstAttack() //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        if (m_actor.contacts.below) {
            AttackMove();
        }
        m_currentAngle = m_attackAngle;
        m_attackNumber = 1;
        m_currentlyAttacking = true;
        m_alreadyAttacked = false;
        m_currentHitPriority = 1;

        yield return new WaitForSeconds(m_meleeAttackTime);
        m_currentlyAttacking = false;
    }

    private IEnumerator SecondAttack() //darauf achten während dem air attack etwas gravity dazuzurechnen --> nicht nötig weil movement in air nicht disabled ist
    {
        if (m_actor.contacts.below) {
            AttackMove();
        }
        m_currentAngle = m_attackAngle;
        m_attackNumber = 2;
        m_currentlyAttacking = true;
        m_alreadyAttacked = false;
        m_attackDirection = RotateVector(m_attackDirection, 20);
        m_currentHitPriority = 2;

        yield return new WaitForSeconds(m_meleeAttackTime);
        m_currentlyAttacking = false;
    }
    private IEnumerator ThirdAttack() //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        m_comboActive = false;
        if (m_actor.contacts.below) {
            AttackMove();
        }
        m_currentAngle = m_attackAngle;
        m_currentlyAttacking = true;
        m_alreadyAttacked = false;
        m_attackDirection = RotateVector(m_attackDirection, -40);
        m_currentHitPriority = 3;

        yield return new WaitForSeconds(m_meleeAttackTime);
        m_pm.DisableUserInput(false);
        m_currentlyAttacking = false;
        m_attackNumber = 0;
        StopCoroutine(m_meleeAttack);
        m_meleeAttack = null;
        m_currentAttackState = AttackState.None; //function end attack schreiben
        StartCoroutine(AttackCooldown());
        m_currentHitPriority = 1;
    }

    private void VisualizeAttack(Vector2 _direction)
    {
        Vector2 directionLine = _direction.normalized * m_attackRange;
        Vector2 leftArc = RotateVector(directionLine, m_attackAngle);
        Vector2 rightArc = RotateVector(directionLine, -m_attackAngle);

        Vector2 hitVisual = RotateVector(directionLine, m_currentAngle);
        //CurrentAngle -= (AttackAngle*2) -  1 / FirstAttackTime; //Time.deltaTime?
        m_currentAngle -= 1 * 1 / m_meleeAttackTime;

        Debug.DrawLine(transform.position, (Vector2)transform.position + hitVisual, Color.red);
        Debug.DrawLine(transform.position, (Vector2)transform.position + directionLine, Color.green);
        Debug.DrawLine(transform.position, (Vector2)transform.position + leftArc, Color.green);
        Debug.DrawLine(transform.position, (Vector2)transform.position + rightArc, Color.green);
    }

    private void StartMeteorSmash() //rename
    {
        m_invincible = true;
        m_currentAttackState = AttackState.Smash;
        Vector2 velocityDown = Vector2.down * m_smashSpeed;
        m_pm.DisableUserInput(true);
        m_pm.externalVelocity = velocityDown;
    }

    private void StopMeteorSmash()
    {
        m_pm.DisableUserInput(false);
        m_currentAttackState = AttackState.None;
        StartCoroutine(InvincibilityFrames(m_invincibilityTime));
    }

    private Vector2 RotateVector(Vector2 _v, float _degrees)
    {
        float sin = Mathf.Sin(_degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(_degrees * Mathf.Deg2Rad);

        float tx = _v.x;
        float ty = _v.y;
        _v.x = (cos * tx) - (sin * ty);
        _v.y = (sin * tx) + (cos * ty);
        return _v;
    }

    private IEnumerator InvincibilityFrames(float _duration)
    {
        m_invincible = true;
        yield return new WaitForSeconds(_duration);
        m_invincible = false;
    }

    private IEnumerator KnockBack(float _repetitions, Vector2 _knockBackOrigin, float _knockBackForce, Enemy _enemy = null) //knock back direction als Parameter übergeben //vllt cancel all movement (hook usw.) einbauen
    {
        //PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Disabled;
        //m_pm.DisableUserInput(true);
        //
        //StartCoroutine(InvincibilityFrames(m_invincibilityTime));
        //
        //for (int i = 0; i < _repetitions; i++) {
        //    float test = 1 - Mathf.Pow((i), 3) / 100;
        //    if (test < 0)
        //        test = 0;
        //    int additionalPosition = 0;
        //    if (Mathf.Abs(transform.position.x - _knockBackOrigin.position.x) < 0.15f) {//KnockBacktolerance or so
        //        additionalPosition = 10;
        //    }
        //    Vector2 KnockBackDirection = (transform.position - new Vector3(_knockBackOrigin.position.x + additionalPosition, _knockBackOrigin.position.y, _knockBackOrigin.position.z)).normalized;
        //    m_pm.externalVelocity = KnockBackDirection * test * _knockBackForce; //currently no gravity? --> wahrscheinlich ne gute idee //funktioniertt das mit der enemy collission?
        //
        //    yield return new WaitForSeconds(0.03f);
        //}
        //m_spriteRenderer.color = m_originalColor;
        //m_colorChangeCounter = 0;
        //PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Waiting;
        //m_pm.DisableUserInput(false);

        PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Disabled;
        m_pm.DisableUserInput(true);
        m_invincible = true;

        Vector2 direction = ((Vector2)transform.position - _knockBackOrigin).normalized;

        if (_enemy) {
            _enemy.frozen = true;
        }

        yield return new WaitForSeconds(m_hitFreezeTime); // Freeze time

        m_pm.externalVelocity = direction * _knockBackForce;

        yield return new WaitForSeconds(m_hitKnockbackTime); // Knockback time

        if (_enemy) {
            _enemy.frozen = false;
        }

        m_pm.momentum = m_pm.externalVelocity;

        m_invincible = false;
        PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Waiting;
        m_pm.DisableUserInput(false);
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void GetHit(Vector2 _knockBackOrigin, float _knockBackForce, Enemy _enemy = null) //bandaid fix for knockbackdirectino //player knockback noch bisshen stärker einstellen //knockback system allgemein überarbeiten
    {
        if (!m_invincible) {
            //vllt die überprüfung ob der hit gilt hier rein machen --> viel besser
            //sinvoll? oder vllt nur get hit wenn knock back aktuell nicht aktiv ist?
            //was ist mit attacksequence usw.? die auch stoppen?
            //StopAllCoroutines(); //wirklich alle stoppen? --> wahrscheinlich sinnvoll
            if (m_knockbackCoroutine != null) {
                StopCoroutine(m_knockbackCoroutine);
            }
            m_knockbackCoroutine = StartCoroutine(KnockBack(10, _knockBackOrigin, _knockBackForce, _enemy));
        }
    }

    //GetHit //Stagger ...
}