using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//vllt collider nur überprüfen wenn der spieler sich bewegt
//reset nearest point wenn nichtmehr in range
//outer circle for hook reset
//cancel old Hook with new Hook
//cancel hook with space == merken wenn der spieler schon vorher space gedrückt hat?
//cancel hook with new hook not working
//for max active time --> invoke Deactivte hook , max active time
//maybe additional travel time --> additional travel distance --> oder additional time durch hookspeed errechnen
//combine velocity with velocity update time
//evlt target hook point nicht nochmal selecten können wenn er schon selected ist
//googlen wie man velocity addiert usw.
//velocity durch clamp beschränken
//cooldown
//cancel hook with new hook wie bei cancel mit jump erst nach einer gewissen distanz
//combine velocity erstmal raus --> überlegen ob es sinnvoll sein könnte
//evtl zusätzliche distanz wieder auf prozent umstellen
//cooldown noch überlegen wie
//smooth rotation with joystick: currentSelectDirection speichern , actualSelectDirection anschauen und vergleichen --> über zeit lerpen? / lerp/smooth factor einbauen // alternativ rotationslösung einbauen --> winkel zwischen Vector2.zero und dem aktuellen vector speichern 
//die letzen 16 winkel speicher und über lerp ineinander smoothen
//visualize with colorlerp
//evtl: distanz zwischen spieler und ziel ausrechnen, distanz pro frame ausrechnen --> frames bis ziel ausrechnen, danach abbrehcen
//cancel Hook with new hook --> bewegung des spielers möglich wenn hook gecancellt wurde und keien neuer hook point gefunden wurde
//cancel with jump bug
//throw hook in direction --> even if theres no target point --> fist attempt at cooldown
//cancel with time funktioniert aktuell nur mit 60 fps
//cooldown errechnet sich aus hook range und repeat time
//vllt immer in facing direction hooken --> um das controller input axis = 0,0 zu beheben
//immer überprüfen ob sich die tags geändert haben
//hook state --> nur checken wenn hook nicht active ist
//pull object to player --> object stays with player --> throw object
//vllt während das object auf den spieler zufliegt layer collission ausmachen damit es nirgendwo hängen bleiben kann
//evtl passen visuals bei dem wurf nicht zusammen weil die visuals andere werte haben als der wurf
//default values für alles einstellen
//getter und setter einbauen
//direction probleme --> wegen mouse

//requires component player movement, + ...
//für später eine set hookstate funktion schreiben
//vllt für jump back wie bei hook to target eine eigene funktion schreiben 
//small enemy?
//kleiner bug bei bigenemy
//wenn man die timescale wieder progressive normalisiert unbedingt drauf achten das sie zu 100% wieder normal ist sobald man irgendetwas machen kann das nicht in diesem script passiert
public class PlayerHook : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    public enum PlayerState //Später in das Player Anim Script --> bzw. an einem besseren Ort managen
    {
        Waiting,
        Hook,
        Attacking,
        Moving,
        Disabled,
        Invincible,
        Dead
    }

    public enum TimeSlow
    {
        NoSlow,
        Instant,
        SlowFast,
        FastSlow
    }

    enum HookState //brauch ich starting überhaupt, brauch ich evtl aiming?
    {
        Inactive,
        SearchTarget,
        Aiming,
        SwitchTarget,
        Active,
        Cooldown,
        JumpBack
    }

    enum HookType
    {
        None,
        Throw,
        Pull,
        Hook,
        BigEnemy
    }

    //***********//
    //    ???    //
    //***********//

    public static PlayerState CurrentPlayerState = PlayerState.Waiting; //s. oben //nicht vergessen den playerstate auch zu benutzen

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float m_hookRadius = 6f;
    [SerializeField] private float m_safetyRadius = 1f; //besseren namen finden
    [SerializeField] private float m_angle = 20f;
    [SerializeField] private float m_pullAngle = 5f; //renamen
    [SerializeField] private float m_hookSpeed = 15f;
    [SerializeField] private float m_pullSpeed = 15f;
    [SerializeField] private float m_maxTimeSlow = 0.1f; //TimeSlowPercent
    [SerializeField] private float m_maxTimeActive = 2;
    [SerializeField] private float m_targetReachedTolerance = 0.2f; //wie nah muss man am ziel sein damit der hook abbricht
    [SerializeField] private bool m_cancelHookWithSpace = true;
    [SerializeField] private bool m_cancelHookWithNewHook = true;
    [SerializeField] private float m_cancelDistancePercentage = 0.5f; //wie viel prozent des abstands der spieler geschafft haben muss bevor er den hook abbrechen kann
    [SerializeField] private TimeSlow m_formOfTimeSlow = TimeSlow.FastSlow;
    //[SerializeField] private GameObject m_radiusVisualization = default; //rename
    [SerializeField] private GameObject m_hookPointVisualization = default; //rename
    [SerializeField] private LayerMask m_hookPointFilter = default; //Filter Layer to only get Hook Points in Sight
    [SerializeField] private LayerMask m_hookPointLayer = default; //default layer einstellen
    [SerializeField] private bool m_slowTimeWhileAiming = default;

    [SerializeField] private float m_minThrowVelocity = 15f;
    [SerializeField] private float m_maxThrowVelocity = 25f;

    [SerializeField] private float m_maxTimeToWinRopeFight = 3f;
    [SerializeField] private int m_numOfButtonPresses = 10;
    //[SerializeField] private float m_contrAdditionalPullAngle = 30f; //angles können eigentlich int sein
    [SerializeField] private bool m_usingController = false;
    [SerializeField] private bool m_useSmartTargetingForEverything = false;

    //experimental
    [SerializeField] private float m_dashBoostActiveTime = 0.3f;
    [SerializeField] [Range(0, 2)] private float m_dashSpeedMultiplier = 1f;


    //**********************//
    //    Private Fields    //
    //**********************//

    private HookState m_currentHookState = HookState.Inactive;
    private HookType m_currentTargetType = HookType.None;
    private float m_currentTimeActiveRope;
    private int m_buttonPresses;

    private List<Collider2D> m_totalHookPoints;

    private float m_normalTimeScale;
    private float m_cancelDistance;
    private float m_controllerDeadzone; //additional deadzone different from the input deadzone --> for better targeting controll
    private bool m_reachedTarget; //ob das ziel erreicht wurde

    private float m_timeSlowTest; //test for one variant of the progressive timeslow
    private float m_currentTimeActive;

    private Collider2D m_currentSelectedTarget; // Position des HookPoints
    private Collider2D m_currentSwitchTarget; //aktuelles ziel im cone
    private Vector2 m_targetPosition; //glaube 1 von beiden reicht
    private Vector2 m_currentTargetPosition;
    private Vector2 m_startPosition;

    private Vector2 m_controllerDirection;
    private Vector2 m_contDirWithoutDeadzone;
    private Vector2 m_mouseDirection;
    private Vector2 m_throwVelocity;

    private Coroutine m_hookCooldown = null;

    private GameObject m_pickedUpObject;

    private PlayerInput m_input;
    private Actor2D m_actor;
    private PlayerMovement m_pm;
    private Rigidbody2D m_rb;


    //experimental

    private bool m_activatedAfterHookDash;
    private Vector2 m_dashDirection;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {
        m_controllerDeadzone = 0.5f;
        m_normalTimeScale = Time.timeScale;
        m_totalHookPoints = new List<Collider2D>();
        m_controllerDirection = Vector2.zero;

        m_input = GetComponent<PlayerInput>();
        m_actor = GetComponent<Actor2D>();
        m_pm = GetComponent<PlayerMovement>();
        m_rb = GetComponent<Rigidbody2D>();

        if (m_hookPointVisualization != null)
            m_hookPointVisualization.GetComponent<HookPointVisualization>().SetObjectScale(m_hookRadius);
    }

    void Update()
    {
        if (CurrentPlayerState == PlayerState.Waiting || CurrentPlayerState == PlayerState.Hook) //darauf achten das es auch den player state moving gibt
        {
            SetPlayerState();

            if (!m_usingController)
            {
                Vector2 mousePosition = Input.mousePosition;
                mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                m_mouseDirection = (mousePosition - (Vector2)transform.position).normalized;
            }
            else
            {
                if (m_input.player.GetAxis(m_input.aimHorizontalAxis) < -m_controllerDeadzone || m_input.player.GetAxis(m_input.aimHorizontalAxis) > m_controllerDeadzone || m_input.player.GetAxis(m_input.aimVerticalAxis) < -m_controllerDeadzone || m_input.player.GetAxis(m_input.aimVerticalAxis) > m_controllerDeadzone)
                {
                    //vllt nur überprüfen ob die insgesamte auslenkung (Mathf.Abs(horizontal) + mathf.Abs(vertical) > deadzone) ist
                    m_controllerDirection.x = m_input.player.GetAxis(m_input.aimHorizontalAxis);
                    m_controllerDirection.y = m_input.player.GetAxis(m_input.aimVerticalAxis);
                    m_controllerDirection = m_controllerDirection.normalized;
                }
            }

            if (m_input.player.GetAxis(m_input.aimHorizontalAxis) != 0 || m_input.player.GetAxis(m_input.aimVerticalAxis) != 0)
            {
                m_contDirWithoutDeadzone.x = m_input.player.GetAxis(m_input.aimHorizontalAxis);
                m_contDirWithoutDeadzone.y = m_input.player.GetAxis(m_input.aimVerticalAxis);
            }

            if ((m_currentHookState != HookState.Active && m_currentHookState != HookState.Cooldown || CanUseHook()) && m_currentHookState != HookState.JumpBack)
            { //in CanUseHook alle anderen sachen abfragen //--> vllt nur CanUseHook() //CurrentHookState == HookState.Inactive || 
                if (m_input.player.GetButton(m_input.hookButton))
                {
                    if (m_pickedUpObject != null && (m_pickedUpObject.GetComponent<ThrowableObject>().currentObjectState == ThrowableObject.ThrowableState.PickedUp || m_pickedUpObject.GetComponent<ThrowableObject>().currentObjectState == ThrowableObject.ThrowableState.TravellingToPlayer))
                        AimThrow();
                    else
                        SearchTargetPoint();
                }
                else if ((m_input.player.GetButtonUp(m_input.hookButton)) && (m_currentHookState == HookState.SearchTarget || m_currentHookState == HookState.SwitchTarget || m_currentHookState == HookState.Aiming || (m_currentHookState == HookState.Active && CanUseHook())))
                {
                    if (m_currentHookState == HookState.Aiming && m_pickedUpObject != null)
                    {
                        ThrowObject(m_throwVelocity);
                    }
                    else
                    {
                        ActivateHook();
                    }
                    ResetValues();
                }
            }

            if (m_currentHookState == HookState.Active) // || (CurrentHookState == HookState.SwitchTarget && CurrentSelectedTarget != null)
            {
                bool cancelCondition = false;
                switch (m_currentTargetType)
                {
                    case HookType.Hook:
                    case HookType.BigEnemy:
                        {
                            cancelCondition = HookToTarget();
                            break;
                        }
                    case HookType.Pull:
                        {
                            cancelCondition = RopePull();
                            break;
                        }
                    case HookType.Throw:
                        {
                            cancelCondition = PullObject();
                            break;
                        }
                    case HookType.None:
                        {
                            Debug.Log("smth went wrong target should have a type by now");
                            break;
                        }
                }
                if (cancelCondition)
                {
                    //if (m_reachedTarget && m_currentTargetType == HookType.BigEnemy)
                    //{
                    //    StartCoroutine(JumpBack());
                    //}
                    if(m_reachedTarget && m_activatedAfterHookDash && m_currentTargetType == HookType.Hook)
                    {
                        DeactivateHook();
                        GetComponent<PlayerCombat>().DashInDirection(m_dashDirection * m_dashSpeedMultiplier, m_dashBoostActiveTime);
                    }
                    else
                    {
                        DeactivateHook();
                    }
                }
            }
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void SetPlayerState() //vllt muss man die nochmal überarbeiten
    {
        if (m_currentHookState != HookState.Inactive)
        { // funktioniert noch nicht ganz --> nicht jedes frame auf wating setzen //vllt in eigene globale set playerstate function --> die auch nur an einer stelle aufgerufen werden sollte?                                        //oder globale function check player state die den aktuellen state überprüft
            CurrentPlayerState = PlayerState.Hook;
        }
        else
        {
            if (CurrentPlayerState == PlayerState.Hook && m_currentHookState == HookState.Inactive)
            {
                CurrentPlayerState = PlayerState.Waiting;
            }
        }
    }

    private void ThrowObject(Vector2 _throwVelocity)
    {
        GetComponent<VisualizeTrajectory>().RemoveVisualDots();
        m_pickedUpObject.GetComponent<ThrowableObject>().Throw(_throwVelocity);
        m_pickedUpObject = null;
        DeactivateHook();
    }

    private void AimThrow()
    {
        m_currentHookState = HookState.Aiming;
        //Vector2 MomentumVelocity = m_rb.velocity; //freddie fragen
        m_pm.DisableUserInput(true);
        m_pm.externalVelocity = Physics2D.gravity * 0.5f;
        //m_pm.momentum = MomentumVelocity;

        if (m_slowTimeWhileAiming)
        {
            SlowTime();
        }
        if (m_usingController)
        {
            m_throwVelocity = GetAimDirection(m_contDirWithoutDeadzone);
        }
        else
        {
            m_throwVelocity = GetAimDirection(m_mouseDirection); //wird das funktionieren? // wenn ja wie gut? --> funktioniert ganz ok --> bei maus wird immer mit maximaler kraft geworfen
        }
        if (m_input.player.GetButtonDown(m_input.jumpButton))
        {
            m_pickedUpObject.GetComponent<ThrowableObject>().Drop();
            m_pickedUpObject = null;
            DeactivateHook();
            GetComponent<VisualizeTrajectory>().RemoveVisualDots();
        }
    }

    private Vector2 GetAimDirection(Vector2 _direction) //evlt während aim den spieler anhalten oder bewegung verlangsamen //schauen ob man die deadzone lassen kann ansonsten als parameter übergeben
    {
        float velocity = Mathf.Lerp(m_minThrowVelocity, m_maxThrowVelocity, (Mathf.Abs(_direction.x) + Mathf.Abs(_direction.y)));
        Vector2 throwVelocity = new Vector2(_direction.x, _direction.y).normalized * velocity; //falls wir nicht lerpen --> public float ThrowSpeed
        GetComponent<VisualizeTrajectory>().VisualizeDots(transform.position, throwVelocity);
        return throwVelocity;
    }

    private void SlowTime()
    {
        switch (m_formOfTimeSlow)
        {
            case TimeSlow.NoSlow:
                {
                    break;
                }
            case TimeSlow.Instant:
                {
                    Time.timeScale = m_maxTimeSlow;
                    Time.fixedDeltaTime = Time.timeScale * 0.02f;
                    break;
                }
            case TimeSlow.SlowFast:
                {
                    m_timeSlowTest += 0.01f;
                    ProgressiveTimeSlowTwo(m_timeSlowTest);
                    break;
                }
            case TimeSlow.FastSlow:
                {
                    ProgressiveTimeSlow();
                    break;
                }
        }
    }

    private bool PullObject() //hier wahrscheinlihc picked up object setzen sobald throwable object = travelling to player und oben dann nur aim erlauben wenn picked up
    {
        bool cancelCondition = false;
        GetComponentInChildren<DrawLine>().VisualizeLine(transform.position, m_currentSelectedTarget.transform.position);

        if (m_currentSelectedTarget.GetComponent<ThrowableObject>().currentObjectState == ThrowableObject.ThrowableState.PickedUp)
        {
            cancelCondition = true;
            m_pickedUpObject = m_currentSelectedTarget.gameObject;
        }
        return cancelCondition;
    }

    private bool RopePull()
    {
        Debug.DrawLine(transform.position, m_currentSelectedTarget.transform.position, Color.cyan);
        GetComponentInChildren<DrawLine>().VisualizeLine(transform.position, m_currentSelectedTarget.transform.position);

        Vector2 ropeDirection = (m_currentSelectedTarget.transform.position - transform.position).normalized;
        ropeDirection *= -ropeDirection.magnitude; //opposite direction

        if (m_input.player.GetButtonDown(m_input.attackButton))
        { //  && Mathf.Abs(Vector2.Angle(RopeDirection, ControllerDirection.normalized)) < ContrAdditionalPullAngle //-->removed additional pull criteria
            m_buttonPresses--;
            if (transform.position.x > m_currentSelectedTarget.transform.position.x)
            {
                transform.position = new Vector3(transform.position.x + 0.3f, transform.position.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x - 0.3f, transform.position.y, transform.position.z);
            }
            if (transform.position.x != m_currentSelectedTarget.transform.position.x)
            {
                Vector2 newCharacterVelocity = (new Vector2(m_currentSelectedTarget.transform.position.x, 0) - new Vector2(transform.position.x, 0)).normalized * 2;
                m_pm.externalVelocity = newCharacterVelocity;
            }
        }

        bool CancelCondition = false;
        if (m_buttonPresses <= 0)
        {
            //m_currentSelectedTarget.GetComponent<PullableObject>().Funktionsname();
            if (m_currentSelectedTarget.transform.parent != null && m_currentSelectedTarget.transform.parent.GetComponent<Enemy>() != null)
            { //hier kommt später die funktion hin die regelt was passiert wenn der spieler gewinnt
                m_currentSelectedTarget.transform.parent.GetComponent<Enemy>().GetHit(transform.position, 15, 4); //4 evtl auch als parameter hit priority übergeben
            }
            CancelCondition = true;
        }

        m_currentTimeActiveRope += Time.deltaTime / Time.timeScale;
        if (m_currentTimeActiveRope > m_maxTimeToWinRopeFight)
        {
            GetComponent<PlayerCombat>().GetHit(m_currentSelectedTarget.transform.position, 10);
            CancelCondition = true;
        }
        if (CancelCondition)
        {
            m_currentTimeActiveRope = 0;
        }
        return CancelCondition;
    }

    private void DeactivateHook(bool _disableInput = false)
    {
        m_currentHookState = HookState.Inactive;
        m_currentTargetType = HookType.None;
        m_currentSelectedTarget = null; //vllt brauch ich das gar nicht ? //evlt nur currentselectedpoint == null
        m_currentSwitchTarget = null;
        m_pm.DisableUserInput(_disableInput);

        GetComponentInChildren<DrawLine>().ActivateVisualization(false);
        ResetValues(); //weiß nicht ob das sogut ist?
        //evlt stop all coroutines? --> falls es von einem anderen script her aufgerufen wird
    }

    private void CheckTargetType(Collider2D _target)
    {
        if (_target.CompareTag("HookPoint"))
        {
            if (_target.transform.parent != null && _target.transform.parent.CompareTag("BigEnemy"))
            { //brauch ihc einen parent null check?
                m_currentTargetType = HookType.BigEnemy;
            }
            else
            {
                m_currentTargetType = HookType.Hook;
            }
        }
        if (_target.CompareTag("Throwable"))
        {
            m_currentTargetType = HookType.Throw;
        }
        if (_target.CompareTag("RopePoint"))
        {
            m_currentTargetType = HookType.Pull;
        }
    }

    private void ResetValues()
    {
        if (m_hookPointVisualization != null)
        {
            m_hookPointVisualization.GetComponent<HookPointVisualization>().ActivateVisuals(false);
        }

        m_timeSlowTest = 0; //wofür?
        ResetHookPoints();
        m_totalHookPoints.Clear();
        Time.timeScale = m_normalTimeScale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        m_currentTimeActive = 0;
    }

    private void SearchTargetPoint()
    {
        if (m_currentHookState != HookState.Active && m_currentHookState != HookState.SwitchTarget)
        {
            m_currentHookState = HookState.SearchTarget;
        }

        if (m_hookPointVisualization != null)
        {
            m_hookPointVisualization.GetComponent<HookPointVisualization>().ActivateVisuals(true);
        }

        Vector2 direction = m_usingController ? m_controllerDirection : m_mouseDirection;

        if (m_currentHookState == HookState.SearchTarget)
        {
            m_currentSelectedTarget = FindNearestTargetInRange(direction);
        }
        else if (m_currentSelectedTarget != FindNearestTargetInRange(direction))
        { // && FindNearestTargetInRange(MouseDirection).CompareTag("HookPoint")
            if (FindNearestTargetInRange(direction) != null && FindNearestTargetInRange(direction).CompareTag("HookPoint"))
            {
                m_currentSwitchTarget = FindNearestTargetInRange(direction);
                m_currentHookState = HookState.SwitchTarget;
            }
            else if (FindNearestTargetInRange(direction) == null)
            {
                m_currentSwitchTarget = FindNearestTargetInRange(direction);
                m_currentHookState = HookState.SwitchTarget;
            }
        }
        VisualizeCone(direction);

        if (m_hookPointVisualization != null)
        {
            m_hookPointVisualization.GetComponent<HookPointVisualization>().SetPointerDirection(direction);
        }


        SlowTime();
        m_currentTimeActive += Time.deltaTime / Time.timeScale;
        if (m_currentTimeActive > m_maxTimeActive)
        {
            ResetValues();
            ActivateHook();
        }
    }

    private void ActivateHook() //_direction wahrscheinlich unnötig
    {
        if (m_currentHookState != HookState.Active)
        {
            if (m_currentSwitchTarget != null)
            {
                m_currentSelectedTarget = m_currentSwitchTarget;
            }
            m_currentHookState = HookState.Active;
            m_startPosition = transform.position; //aktuell nur für den hookabbruch bug bei keiner velocity
            if (m_currentSelectedTarget != null) //evtl alle noch vorherigen sachen resetten?
            {
                //Debug.Log("hier"); //resettet alle values wenn auch wenn man nicht switched --> evtl nur if hookstate != active und active erst am ende der funktion setzen
                m_targetPosition = m_currentSelectedTarget.transform.position;
                CheckTargetType(m_currentSelectedTarget);
                switch (m_currentTargetType)
                {
                    case HookType.Hook:
                        {

                            //experimental
                            m_activatedAfterHookDash = false;
                            m_dashDirection = Vector2.zero;

                            m_reachedTarget = false; //evtl alles in einer funktion machen lassen für moving hookpoint
                            m_currentTargetPosition = m_currentSelectedTarget.transform.position;
                            m_cancelDistance = CalculateCancelDistance(transform.position, m_currentSelectedTarget.transform.position); //beachtet die zusätzliche TravelDistanceNicht
                            m_pm.DisableUserInput(true);
                            SetVelocityTowardsTarget(m_currentTargetPosition, m_hookSpeed);
                            break;
                        }
                    case HookType.BigEnemy:
                        {
                            m_reachedTarget = false; //evtl alles in einer funktion machen lassen für moving hookpoint
                            m_currentTargetPosition = m_currentSelectedTarget.transform.position;
                            m_cancelDistance = CalculateCancelDistance(transform.position, m_currentSelectedTarget.transform.position); //beachtet die zusätzliche TravelDistanceNicht
                            m_pm.DisableUserInput(true);
                            SetVelocityTowardsTarget(m_currentTargetPosition, m_hookSpeed);
                            break;
                        }
                    case HookType.Pull:
                        {
                            m_buttonPresses = m_numOfButtonPresses;
                            m_pm.DisableUserInput(true); // kann evtl nach oben
                            m_pm.externalVelocity = Physics2D.gravity * 0.5f; // kann evtl nach oben
                            break;
                        }
                    case HookType.Throw:
                        {
                            m_currentSelectedTarget.GetComponent<ThrowableObject>().PickUp(transform, m_pullSpeed, m_targetReachedTolerance);
                            break;
                        }
                }
            }
            else
            {
                m_currentHookState = HookState.Cooldown;
                Vector2 hookDirection = Vector2.zero;
                if (m_usingController == false)
                {
                    hookDirection = m_mouseDirection;
                }
                else
                {
                    hookDirection = m_controllerDirection;
                }
                m_hookCooldown = StartCoroutine(ThrowHook(hookDirection));
            }
        }
    }

    private bool HookToTarget()
    {
        Debug.DrawLine(transform.position, m_currentSelectedTarget.transform.position);
        GetComponentInChildren<DrawLine>().VisualizeLine(transform.position, m_currentSelectedTarget.transform.position);

        bool cancelCondition = false;

        if (Vector2.Distance(transform.position, m_currentTargetPosition) < m_targetReachedTolerance)
        { //wenn man sein ziel erreicht hat
            m_reachedTarget = true;
            cancelCondition = true;

            //experimental
            if (m_activatedAfterHookDash)
                m_dashDirection = m_pm.externalVelocity;
        }

        if (m_cancelHookWithSpace && (m_input.player.GetButton(m_input.jumpButton)) && Vector2.Distance(transform.position, m_currentSelectedTarget.transform.position) < m_cancelDistance)
        { //falls aktiviert: wenn space gedrückt und bereits ein prozentualer teil des weges erreich wurde
            cancelCondition = true;
        }

        if (m_input.player.GetButton(m_input.attackButton) && Vector2.Distance(transform.position, m_currentSelectedTarget.transform.position) < m_cancelDistance)
        {
            m_activatedAfterHookDash = true;
        }

        if (m_rb.velocity == Vector2.zero && m_startPosition != (Vector2)transform.position)
        {
            cancelCondition = true;
        }

        if (m_reachedTarget == false) //hier könnte der hook bug liegen --> unter gewissen umständen aktualisiert er zu oft
        {
            m_targetPosition = m_currentSelectedTarget.transform.position;
            m_currentTargetPosition = m_currentSelectedTarget.transform.position;
            SetVelocityTowardsTarget(m_currentTargetPosition, m_hookSpeed);
        }

        return cancelCondition;
    }

    private void ProgressiveTimeSlowTwo(float _x) 
    {
        if (Time.timeScale > m_maxTimeSlow)
        {
            Time.timeScale -= Mathf.Pow(_x, 2);
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
    }

    private void ProgressiveTimeSlow() //fast slowvoid
    {
        if (Time.timeScale > m_maxTimeSlow)
        {
            Time.timeScale *= 0.93f;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
    }

    private IEnumerator ThrowHook(Vector2 _direction)
    {
        int test = 0;
        while (test < m_hookRadius)
        {
            Vector2 temp = _direction * test;
            Debug.DrawLine(transform.position, (Vector2)transform.position + temp);
            GetComponentInChildren<DrawLine>().VisualizeLine(transform.position, (Vector2)transform.position + temp);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, _direction, test, m_hookPointFilter); //weil player nichtmehr auf dem ignore raycast layer ist
            if (hit.collider != null)
            {
                StartCoroutine(PullBackHook(_direction, test));
                StopCoroutine(m_hookCooldown);
            }
            test++;
            yield return new WaitForSeconds(0.03f);
        }
        StartCoroutine(PullBackHook(_direction, (int)m_hookRadius));
    }

    private IEnumerator PullBackHook(Vector2 _direction, int _numberOfRepeats)
    {
        for (int i = _numberOfRepeats; i > 0; i--)
        {
            Vector2 temp = _direction * i;
            Debug.DrawLine(transform.position, (Vector2)transform.position + temp, Color.red);
            GetComponentInChildren<DrawLine>().VisualizeLine(transform.position, (Vector2)transform.position + temp);
            yield return new WaitForSeconds(0.03f);
        }
        m_currentHookState = HookState.Inactive;
        GetComponentInChildren<DrawLine>().ActivateVisualization(false);
    }

    private bool CanUseHook()
    {
        if (m_cancelHookWithNewHook)
        {
            if (m_currentSelectedTarget != null && Vector2.Distance(transform.position, m_currentSelectedTarget.transform.position) < m_cancelDistance && m_currentTargetType != HookType.Throw)
            {
                return true;
            }
        }
        return false;
    }

    private float CalculateCancelDistance(Vector2 _startPoint, Vector2 _endPoint)
    {
        float totalDistance = Vector2.Distance(_startPoint, _endPoint);
        totalDistance *= 1 - m_cancelDistancePercentage;
        return totalDistance;
    }

    private IEnumerator JumpBack() //--> gibt noch bugs 
    {
        int x = 1;
        if (transform.position.x > m_currentSelectedTarget.transform.parent.transform.position.x)
        {
            x = 1;
        }
        else
        {
            x = -1;
        }

        DeactivateHook();
        m_pm.DisableUserInput(true);
        m_currentHookState = HookState.JumpBack;
        Vector2 jumpBackvelocity = new Vector2(0.5f * x, 0.5f).normalized * m_hookSpeed; //evtl jump speed
        m_pm.externalVelocity = jumpBackvelocity;
        yield return new WaitForSeconds(0.4f * 10 / m_hookSpeed); //bessere lösung finden --> passt fürs erste
        DeactivateHook();
    }

    private void SetVelocityTowardsTarget(Vector2 _targetPoint, float _speed) //rename //evlt speed auch als parameter übergeben
    {
        Vector2 newCharacterVelocity = (_targetPoint - (Vector2)transform.position).normalized * _speed;
        m_pm.externalVelocity = newCharacterVelocity;
        m_pm.momentum = newCharacterVelocity;
    }

    private Collider2D FindNearestTargetInRange(Vector2 _searchDirection) //evtl besser als find target oder so --> noch überlegn wie man die vector2.zero geschichte besser lösen könnte //not working? //hier evtl einen kleinen kreis als absicherung bei sehr nahen hookpoints --> cone erst aber einer gewissen distanz
    {//vllt mit nur einer liste arbeiten und nach und nach sachen entfernen?
        if (_searchDirection == Vector2.zero)
        {
            return null;
        }

        Collider2D[] hookPointsInRange = Physics2D.OverlapCircleAll(transform.position, m_hookRadius, m_hookPointLayer); //hookPoints in range, .... --> hook points in sight
        for (int i = 0; i < hookPointsInRange.Length; i++)
        {
            if (!m_totalHookPoints.Contains(hookPointsInRange[i]))
            {
                m_totalHookPoints.Add(hookPointsInRange[i]);
            }
        }
        ResetHookPoints();

        List<Collider2D> hookPointsInSight = new List<Collider2D>();
        for (int i = 0; i < hookPointsInRange.Length; i++)
        {
            float rayCastLength = Vector2.Distance(transform.position, hookPointsInRange[i].transform.position);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (hookPointsInRange[i].transform.position - transform.position), rayCastLength, m_hookPointFilter);
            if (hit == false)
            {
                if (hookPointsInRange[i].CompareTag("Throwable") && hookPointsInRange[i].GetComponent<ThrowableObject>().currentObjectState == ThrowableObject.ThrowableState.Inactive) //noch keine sogute lösung
                {
                    hookPointsInSight.Add(hookPointsInRange[i]);
                }
                else if (!hookPointsInRange[i].CompareTag("Throwable"))
                {
                    hookPointsInSight.Add(hookPointsInRange[i]);
                }
            }
        }

        List<Collider2D> hookPointsInCone = new List<Collider2D>(); //evlt einen besseren namen finden
        for (int i = 0; i < hookPointsInSight.Count; i++) //wie am besten den extra filter applyen?
        {
            VisualizeLine(transform.position, hookPointsInSight[i].transform.position);
            hookPointsInSight[i].GetComponent<SpriteRenderer>().color = Color.red;
            Vector2 playerToColliderDirection = (hookPointsInSight[i].transform.position - transform.position).normalized;
            float angleInDeg = Vector2.Angle(playerToColliderDirection, _searchDirection);

            if (angleInDeg < m_angle || (Vector2.Distance(transform.position, hookPointsInSight[i].transform.position) < m_safetyRadius && (hookPointsInSight[i].CompareTag("Throwable") || m_useSmartTargetingForEverything)))
            {
                hookPointsInCone.Add(hookPointsInSight[i]);
            }
        }

        if (hookPointsInCone.Count == 0)
        {
            return null;
        }

        Collider2D nearestTargetPoint = new Collider2D();
        float lowestAngle = Mathf.Infinity;
        bool onlyThrowableObjectsInCone = CheckHookPointsInCone(hookPointsInCone);
        for (int i = 0; i < hookPointsInCone.Count; i++) //bisher kein check ob throwable or not --> siehe old function
        {
            Vector2 playerToColliderDirection = (hookPointsInCone[i].transform.position - transform.position).normalized;
            float angleInDeg = Vector2.Angle(playerToColliderDirection, _searchDirection);
            if (angleInDeg < lowestAngle)
            {
                if (hookPointsInCone.Count > 1 && onlyThrowableObjectsInCone == false)
                {
                    if (hookPointsInCone[i].CompareTag("Throwable")) //evlt noch ne bessere lösung finden
                    {
                        if (angleInDeg < m_pullAngle)
                        {
                            lowestAngle = angleInDeg;
                            nearestTargetPoint = hookPointsInCone[i];
                        }
                    }
                    else
                    {
                        lowestAngle = angleInDeg;
                        nearestTargetPoint = hookPointsInCone[i];
                    }
                }
                else
                {
                    lowestAngle = angleInDeg;
                    nearestTargetPoint = hookPointsInCone[i];
                }
            }
        }

        if (nearestTargetPoint != null)
        {
            nearestTargetPoint.GetComponent<SpriteRenderer>().color = Color.green;
        }
        return nearestTargetPoint;
    }

    private bool CheckHookPointsInCone(List<Collider2D> _hookPointsInCone) //returns true if all objects in cone are throwable (better targeting for multiple throwable objects in cone)
    {
        for(int i = 0; i < _hookPointsInCone.Count; i++)
        {
            if (_hookPointsInCone[i].CompareTag("Throwable") == false)
                return false;
        }
        return true;
    }

    private void ResetHookPoints() //müssten eigentlich nur hookpoints in TotalHookPoints sein
    {
        for (int i = 0; i < m_totalHookPoints.Count; i++)
        {
            if (m_totalHookPoints[i] != null)
                m_totalHookPoints[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
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

    private void VisualizeCone(Vector2 _direction)
    {
        Vector2 directionLine = _direction.normalized * m_hookRadius;
        Vector2 leftArc = RotateVector(directionLine, m_angle);
        Vector2 rightArc = RotateVector(directionLine, -m_angle);
        Vector2 innerLeftArc = RotateVector(directionLine, m_pullAngle);
        Vector2 innerRightArc = RotateVector(directionLine, -m_pullAngle);

        Debug.DrawLine(transform.position, (Vector2)transform.position + directionLine);
        Debug.DrawLine(transform.position, (Vector2)transform.position + leftArc);
        Debug.DrawLine(transform.position, (Vector2)transform.position + rightArc);
        Debug.DrawLine(transform.position, (Vector2)transform.position + innerLeftArc, Color.blue);
        Debug.DrawLine(transform.position, (Vector2)transform.position + innerRightArc, Color.blue);
    }

    private void VisualizeLine(Vector2 _start, Vector2 _end) //eigentlich kein guter name --> weil es nur eine bestimmte linie visualisiert
    {
        Debug.DrawLine(_start, _end);
        float visualDistance = Vector2.Distance(_start, _end) * m_cancelDistancePercentage;
        Debug.DrawLine(_start, _start + (_end - _start).normalized * visualDistance, Color.red);
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void CancelHook()
    {
        DeactivateHook(true);
        GetComponent<VisualizeTrajectory>().RemoveVisualDots(); //vllt auch in deactivate hook?
        if (m_pickedUpObject != null)
        {
            m_pickedUpObject.GetComponent<ThrowableObject>().Drop();
            m_pickedUpObject = null;
        }
    }
}




