using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//smash down stärke von der spieler höhe/falllänge abhängig machen
//melee attack: kein input: stehenbleiben, input: bewegung in input richtung --> am besten wie beim knock back 
//default values
//evtl combo time kürzer machen aber dafür schon kurz vor ende der attack input annehmen
public class PlayerCombat : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    public enum AttackState //attack state in player state ändern
    {
        None,
        Attack,
        Dash,
        Smash
    }

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private PlayerAttributes m_attributes = default;
    [SerializeField] private TextMeshProUGUI m_healthVisualization = default;

    //[SerializeField] private LayerMask m_layerMask = default; //später renamen --> enemy hit mask oder so //ground ist wichtig das man gegner nicht durch wände schlagen kann
    // [SerializeField] private float m_controllerTolerance = 0.5f; //brauch ich die noch?

    [SerializeField] private float m_dashSpeed = 20f;
    [SerializeField] private float m_dashDuration = 0.2f;
    [SerializeField] private float m_dashCooldown = 0.5f;

    [SerializeField] private float m_hitFreezeTime = 0.5f;
    [SerializeField] private float m_hitKnockbackTime = 0.2f;
    [SerializeField] private float m_invincibilityTime = 1f;

    //******************//
    //    Properties    //
    //******************//

    public AttackState currentAttackState
    {
        get { return m_currentAttackState; }
    }

    public int currentHealth
    {
        get { return m_currentHealth; }
        set { m_currentHealth = value; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private AttackState m_currentAttackState = AttackState.None;


    private int m_currentHealth;
    //private float m_attackAngle = 25f; //veraltet nur noch füs visuelle da

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
    //private int m_attackNumber;
    //private Coroutine m_meleeAttack;
    //private Coroutine m_meleeMovement;
    //private bool m_currentlyAttacking;
    //private float m_currentAngle;
    //private bool m_alreadyAttacked;
    //private bool m_attackCoolDownActive; //--> evlt später eigenen state für cooldown einbauzne
    private bool m_dashCoolDownActive; //--> irgendwie besser lösen
    //private bool m_comboActive;

    private bool m_invincible;

    //private int m_currentHitPriority = 1;

    private Coroutine m_knockbackCoroutine = null;
    private Coroutine m_dashCoroutine = null;

    bool m_hookDashActive;

    private PlayerInput m_input;
    private Actor2D m_actor;
    private PlayerMovement m_pm;
    private SpriteRenderer m_spriteRenderer;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {

    }

    //cooldown on melee attack? --> allgemein nach jedem angriff kurz 0.4f sec oder so wartezeit?
    private void Start()
    {
        m_originalColor = GetComponent<SpriteRenderer>().color;
        m_enemiesHit = new List<Collider2D>();
        m_xAxis = Input.GetAxis("Horizontal");

        m_input = GetComponent<PlayerInput>();
        m_actor = GetComponent<Actor2D>();
        m_pm = GetComponent<PlayerMovement>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateHealthVisual();
    }

    private void Update() //evlt switch case für attack einbauen --> man kann nicht gleichzeitig meteor smash machen und attacken
    {
        if (PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Waiting || PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Attacking)
        {
            SetPlayerState();
            SetFacingDirection(); //ist es klug jedes frame zu setzen? --> wenn ja so einbauen das es auch funktioniert 
            if (m_input.player.GetButtonDown(m_input.dashButton) && m_currentAttackState == AttackState.None && m_dashCoolDownActive == false)
            {  //&& (Input.GetAxis("Horizontal") < ControllerTolerance || Input.GetAxis("Horizontal") > ControllerTolerance)
                m_dashCoroutine = StartCoroutine(Dash());
            }
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void SetPlayerState() //s. Playerhook
    {
        if (m_currentAttackState != AttackState.None)
        {
            PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Attacking;
        }
        else
        {
            if (PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Attacking && m_currentAttackState == AttackState.None)
            {
                PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Waiting;
            }
        }
    }

    private IEnumerator Dash()
    {
        m_dashCoolDownActive = true;
        Vector2 velocity = Vector2.zero;
        m_currentAttackState = AttackState.Dash;
        SetFacingDirection();
        if (m_facingLeft)
            velocity = Vector2.left; // immer auf die timescale achten wegen dem hook / Time.timeScale --> kann man überhaupt während einem timeslow dashen?
        else
            velocity = Vector2.right;

        m_pm.DisableUserInput(true);
        m_pm.externalVelocity = velocity * m_dashSpeed;
        yield return new WaitForSeconds(m_dashDuration);
        m_pm.DisableUserInput(false);
        m_currentAttackState = AttackState.None;
        yield return new WaitForSeconds(m_dashCooldown);
        m_dashCoolDownActive = false;
    }

    private IEnumerator HookDash(Vector2 _velocity, float time) //only experimental //damage enemies
    {
        m_hookDashActive = true;
        m_invincible = true;
        GetComponentInChildren<PlayerKnockback>().IsLethal(true);

        Vector2 velocity = Vector2.zero;
        m_currentAttackState = AttackState.Dash;

        m_pm.DisableUserInput(true);
        m_pm.externalVelocity = _velocity;
        yield return new WaitForSeconds(time);
        m_pm.DisableUserInput(false);
        m_currentAttackState = AttackState.None;

        GetComponentInChildren<PlayerKnockback>().IsLethal(false);
        m_invincible = false;
        m_hookDashActive = false;
    }

    private void CancelDash()
    {
        m_pm.externalVelocity = Vector2.zero;
        StopCoroutine(m_dashCoroutine);
        m_currentAttackState = AttackState.None;
        m_pm.DisableUserInput(false);
        m_dashCoolDownActive = false;
    }

    private void SetFacingDirection()
    {
        float currentJoystickDirection = m_input.player.GetAxis(m_input.aimHorizontalAxis);
        if (currentJoystickDirection < 0)
        {
            m_facingLeft = true;
        }
        else if (currentJoystickDirection > 0)
        {
            m_facingLeft = false;
        }
    }

    private void TakeDamage()
    {
        m_currentHealth--;
        UpdateHealthVisual();
    }

    void UpdateHealthVisual()
    {
        if (m_healthVisualization != null)
            m_healthVisualization.text = "Health: " + m_currentHealth + " / " + m_attributes.maxHealth;
    }

    private IEnumerator KnockBack(Vector2 _knockBackOrigin, float _knockBackForce, Enemy _enemy = null) //knock back direction als Parameter übergeben //vllt cancel all movement (hook usw.) einbauen
    {
        if (m_currentAttackState == AttackState.Dash)
            CancelDash();
        if (PlayerHook.CurrentPlayerState == PlayerHook.PlayerState.Hook)
            GetComponent<PlayerHook>().CancelHook();

        TakeDamage();
        PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Disabled; //playerstate invincible?
        m_pm.DisableUserInput(true);
        m_invincible = true;
        GetComponent<SpriteRenderer>().color = Color.red; //for visualization

        Vector2 direction = ((Vector2)transform.position - _knockBackOrigin).normalized; //veraltet
        if (_knockBackOrigin.x < transform.position.x)
            direction = new Vector2(0.5f, 0.5f).normalized;
        else
            direction = new Vector2(-0.5f, 0.5f).normalized;

        if (_enemy)
        {
            _enemy.SetFreeze(true);
        }

        yield return new WaitForSeconds(m_hitFreezeTime); // Freeze time

        m_pm.externalVelocity = direction * _knockBackForce;

        yield return new WaitForSeconds(m_hitKnockbackTime); // Knockback time

        if (_enemy)
        {
            _enemy.SetFreeze(false);
        }

        m_pm.momentum = m_pm.externalVelocity;
        m_pm.DisableUserInput(false);

        PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Invincible;

        GetComponent<SpriteRenderer>().color = Color.yellow;

        if (m_currentHealth <= 0) {
            UpdateHealthVisual();
            GameManager.Instance.Respawn();
        }

        yield return new WaitForSeconds(m_invincibilityTime);

        m_invincible = false; //extra invincibility in den knock back einbauen mit extra yield return wait
        PlayerHook.CurrentPlayerState = PlayerHook.PlayerState.Waiting;

        GetComponent<SpriteRenderer>().color = Color.white; // for visualization
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void GetHit(Vector2 _knockBackOrigin, float _knockBackForce, Enemy _enemy = null) //bandaid fix for knockbackdirectino //player knockback noch bisshen stärker einstellen //knockback system allgemein überarbeiten
    {
        if (!m_invincible)
        {
            //vllt die überprüfung ob der hit gilt hier rein machen --> viel besser
            //sinvoll? oder vllt nur get hit wenn knock back aktuell nicht aktiv ist?
            //was ist mit attacksequence usw.? die auch stoppen?
            //StopAllCoroutines(); //wirklich alle stoppen? --> wahrscheinlich sinnvoll

            //evtl stop coroutine dash //+ hier den check machen ob der spieler geknockbacked wird 
            if (m_knockbackCoroutine != null)
            {
                StopCoroutine(m_knockbackCoroutine);
            }
            m_knockbackCoroutine = StartCoroutine(KnockBack(_knockBackOrigin, _knockBackForce, _enemy));
        }
    }

    public void HealUp(int _healValue)
    {
        if ((m_currentHealth + _healValue) <= m_attributes.maxHealth)
            m_currentHealth += _healValue;
        else
            m_currentHealth = m_attributes.maxHealth;
        UpdateHealthVisual();
    }

    public void DashInDirection(Vector2 _velocity, float time)
    {
        if (m_hookDashActive == false)
            StartCoroutine(HookDash(_velocity, time));
    }

    //GetHit //Stagger ...
}