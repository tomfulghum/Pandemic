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
    public enum PlayerState { Waiting, Hook, Attacking, Moving, Disabled } //Später in das Player Anim Script --> bzw. an einem besseren Ort managen
    public enum TimeSlow { NoSlow, Instant, SlowFast, FastSlow }
    enum HookState { Inactive, SearchTarget, Aiming, SwitchTarget, Active, Cooldown, JumpBack } //brauch ich starting überhaupt, brauch ich evtl aiming?
    enum HookType { None, Throw, Pull, Hook, BigEnemy }

    public static PlayerState CurrentPlayerState = PlayerState.Waiting; //s. oben //nicht vergessen den playerstate auch zu benutzen
    public float HookRadius = 6f;
    public float SafetyRadius = 1f; //besseren namen finden
    public float Angle = 20f;
    public float PullAngle = 5f; //renamen
    public float HookSpeed = 15f;
    public float PullSpeed = 15f;
    public float MaxTimeSlow = 0.1f; //TimeSlowPercent
    public float MaxTimeActive = 2;
    public float TargetReachedTolerance = 0.2f; //wie nah muss man am ziel sein damit der hook abbricht
    public bool CancelHookWithSpace = true;
    public bool CancelHookWithNewHook = true;
    public float CancelDistancePercentage = 0.5f; //wie viel prozent des abstands der spieler geschafft haben muss bevor er den hook abbrechen kann
    public float AdditionalTravelDistance = 2f;
    public TimeSlow FormOfTimeSlow = TimeSlow.FastSlow;
    public GameObject RadiusVisualization; //rename
    public LayerMask HookPointFilter; //Filter Layer to only get Hook Points in Sight
    public LayerMask HookPointLayer; //default layer einstellen
    public float ControllerTolerance = 0.25f;
    public bool SlowTimeWhileAiming;

    public float MinThrowVelocity = 5f;
    public float MaxThrowVelocity = 15f;
    public float Gravity = 10f;
    public float MaxFallingSpeed = 15f;

    public float MaxTimeToWinRopeFight = 3f;
    public int NumOfButtonPresses = 10;
    public float ContrAdditionalPullAngle = 30f; //angles können eigentlich int sein
    public float MouseDeadZone = 0.1f;


    HookState CurrentHookState = HookState.Inactive;
    HookType CurrentTargetType = HookType.None;
    float CurrentTimeActiveRope;
    int ButtonPresses;


    List<Collider2D> TotalHookPoints;

    float NormalTimeScale;
    float CancelDistance;
    float FramesTillTarget;
    bool ReachedTarget; //ob das ziel erreicht wurde

    bool UsingController;

    float timeslowTest; //test for one variant of the progressive timeslow
    float CurrentTimeActive;


    Collider2D CurrentSelectedTarget; // Position des HookPoints
    Collider2D CurrentSwitchTarget; //aktuelles ziel im cone
    Vector2 TargetPosition;
    Vector2 CurrentTargetPosition;


    Vector2 ControllerDirection;
    Vector2 ContDirWithoutDeadzone;
    Vector2 LastMousePostion;
    Vector2 MouseDirection;
    Vector2 MousePosition;
    Vector2 ThrowVelocity;

    Vector2 Currentvelocity;

    Coroutine HookCooldown = null;

    GameObject PickedUpObject;
    Actor2D actor;

    // Start is called before the first frame update
    void Start()
    {
        NormalTimeScale = Time.timeScale;
        TotalHookPoints = new List<Collider2D>();
        ControllerDirection = Vector2.zero;
        LastMousePostion = Input.mousePosition;
        actor = GetComponent<Actor2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentPlayerState == PlayerState.Waiting || CurrentPlayerState == PlayerState.Hook) //darauf achten das es auch den player state moving gibt
        {
            SetPlayerState();
            Vector2 CurrentMousePosition = Input.mousePosition;
            if (Mathf.Abs(LastMousePostion.x - CurrentMousePosition.x) > MouseDeadZone || Mathf.Abs(LastMousePostion.y - CurrentMousePosition.y) > MouseDeadZone)
            {
                LastMousePostion = CurrentMousePosition;
                MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                MouseDirection = (MousePosition - (Vector2)transform.position).normalized;
                UsingController = false;
            }

            if (Input.GetAxis("Horizontal") < -ControllerTolerance || Input.GetAxis("Horizontal") > ControllerTolerance || Input.GetAxis("Vertical") < -ControllerTolerance || Input.GetAxis("Vertical") > ControllerTolerance) //extra function --> GetControllerDir
            {
                ControllerDirection.x = Input.GetAxis("Horizontal");
                ControllerDirection.y = Input.GetAxis("Vertical");
                ControllerDirection = ControllerDirection.normalized;
                UsingController = true;
            }
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                ContDirWithoutDeadzone.x = Input.GetAxis("Horizontal");
                ContDirWithoutDeadzone.y = Input.GetAxis("Vertical");
            }

            if ((CurrentHookState != HookState.Active && CurrentHookState != HookState.Cooldown || CanUseHook()) && CurrentHookState != HookState.JumpBack) //in CanUseHook alle anderen sachen abfragen //--> vllt nur CanUseHook() //CurrentHookState == HookState.Inactive || 
            {
                if (Input.GetButton("Hook") || Input.GetAxis("ControllerHook") == 1)
                {
                    if (PickedUpObject != null && (PickedUpObject.GetComponent<ThrowableObject>().CurrentObjectState == ThrowableObject.CurrentState.PickedUp || PickedUpObject.GetComponent<ThrowableObject>().CurrentObjectState == ThrowableObject.CurrentState.TravellingToPlayer))
                        AimThrow();
                    else
                        SearchTargetPoint();
                }
                else if ((Input.GetButtonUp("Hook") || Input.GetAxis("ControllerHook") == 0) && (CurrentHookState == HookState.SearchTarget || CurrentHookState == HookState.SwitchTarget || CurrentHookState == HookState.Aiming || (CurrentHookState == HookState.Active && CanUseHook())))
                {
                    if (CurrentHookState == HookState.Aiming && PickedUpObject != null) //könnte evtl sein das man PickedUpObject in dem Frame gedroppt hat --> relativ sicher
                        ThrowObject(ThrowVelocity);
                    else
                        ActivateHook();
                    ResetValues();
                }
            }

            if (CurrentHookState == HookState.Active) // || (CurrentHookState == HookState.SwitchTarget && CurrentSelectedTarget != null)
            {
                bool CancelCondition = false;
                switch (CurrentTargetType)
                {
                    case HookType.Hook:
                        {
                            CancelCondition = HookToTarget();
                            break;
                        }
                    case HookType.BigEnemy:
                        {
                            CancelCondition = HookToEnemy();
                            break;
                        }
                    case HookType.Pull:
                        {
                            CancelCondition = RopePull();
                            break;
                        }
                    case HookType.Throw:
                        {
                            CancelCondition = PullObject();
                            break;
                        }
                    case HookType.None:
                        {
                            Debug.Log("smth went wrong target should have a type by now");
                            break;
                        }
                }
                if (CancelCondition)
                {
                    if (ReachedTarget && CurrentTargetType == HookType.BigEnemy)
                        StartCoroutine(JumpBack());
                    else
                        DeactivateHook();
                }
            }
        }
    }

    public void CancelHook()
    {
        //StopAllCoroutines(); --> brauch ich das? jump back?
        DeactivateHook();
        GetComponent<VisualizeTrajectory>().RemoveVisualeDots(); //vllt auch in deactivate hook?
    }

    void SetPlayerState() //vllt muss man die nochmal überarbeiten
    {
        if (CurrentHookState != HookState.Inactive) // funktioniert noch nicht ganz --> nicht jedes frame auf wating setzen //vllt in eigene globale set playerstate function --> die auch nur an einer stelle aufgerufen werden sollte?                                        //oder globale function check player state die den aktuellen state überprüft
            CurrentPlayerState = PlayerState.Hook;
        else
            if (CurrentPlayerState == PlayerState.Hook && CurrentHookState == HookState.Inactive)
            CurrentPlayerState = PlayerState.Waiting;
    }

    void ThrowObject(Vector2 _ThrowVelocity)
    {
        GetComponent<VisualizeTrajectory>().RemoveVisualeDots();
        PickedUpObject.GetComponent<ThrowableObject>().Throw(_ThrowVelocity);
        PickedUpObject = null;
        DeactivateHook();
    }

    void AimThrow()
    {
        CurrentHookState = HookState.Aiming;
        if (!actor.collision.below)
            ApplyGravity();
        if (SlowTimeWhileAiming)
            SlowTime();
        if (UsingController)
            ThrowVelocity = GetAimDirection(ContDirWithoutDeadzone);
        else
            ThrowVelocity = GetAimDirection(MouseDirection); //wird das funktionieren? // wenn ja wie gut? --> funktioniert ganz ok --> bei maus wird immer mit maximaler kraft geworfen
        if (Input.GetButtonDown("Fire1"))
        {
            PickedUpObject.GetComponent<ThrowableObject>().Drop();
            PickedUpObject = null;
            DeactivateHook();
            GetComponent<VisualizeTrajectory>().RemoveVisualeDots();
        }
    }

    Vector2 GetAimDirection(Vector2 _direction) //evlt während aim den spieler anhalten oder bewegung verlangsamen //schauen ob man die deadzone lassen kann ansonsten als parameter übergeben
    {
        GetComponent<PlayerMovement>().DisableUserInput(true);
        float velocity = Mathf.Lerp(MinThrowVelocity, MaxThrowVelocity, (Mathf.Abs(_direction.x) + Mathf.Abs(_direction.y)));
        Vector2 throwVelocity = new Vector2(_direction.x, _direction.y).normalized * velocity; //falls wir nicht lerpen --> public float ThrowSpeed
        GetComponent<VisualizeTrajectory>().VisualizeDots(transform.position, throwVelocity, Gravity);
        return throwVelocity;
    }

    void SlowTime()
    {
        switch (FormOfTimeSlow)
        {
            case TimeSlow.NoSlow:
                {
                    break;
                }
            case TimeSlow.Instant:
                {
                    Time.timeScale = MaxTimeSlow;
                    Time.fixedDeltaTime = Time.timeScale * 0.02f;
                    break;
                }
            case TimeSlow.SlowFast:
                {
                    timeslowTest += 0.01f;
                    ProgressiveTimeSlowTwo(timeslowTest);
                    break;
                }
            case TimeSlow.FastSlow:
                {
                    ProgressiveTimeSlow();
                    break;
                }
        }
    }

    bool PullObject() //hier wahrscheinlihc picked up object setzen sobald throwable object = travelling to player und oben dann nur aim erlauben wenn picked up
    {
        bool CancelCondition = false;
        if (CurrentSelectedTarget.GetComponent<ThrowableObject>().CurrentObjectState == ThrowableObject.CurrentState.PickedUp)
        {
            CancelCondition = true;
            PickedUpObject = CurrentSelectedTarget.gameObject;
        }
        return CancelCondition;
    }


    bool RopePull()
    {
        if (!actor.collision.below)
            ApplyGravity();
        Debug.DrawLine(transform.position, CurrentSelectedTarget.transform.position, Color.cyan);
        Vector2 RopeDirection = (CurrentSelectedTarget.transform.position - transform.position).normalized;
        RopeDirection *= -RopeDirection.magnitude; //opposite direction

        if (Input.GetButtonDown("Fire3")) //  && Mathf.Abs(Vector2.Angle(RopeDirection, ControllerDirection.normalized)) < ContrAdditionalPullAngle //-->removed additional pull criteria
        {
            ButtonPresses--;
            if (transform.position.x > CurrentSelectedTarget.transform.position.x)
                transform.position = new Vector3(transform.position.x + 0.2f, transform.position.y, transform.position.z);
            else
                transform.position = new Vector3(transform.position.x - 0.2f, transform.position.y, transform.position.z);
            if (transform.position.x != CurrentSelectedTarget.transform.position.x)
            {
                Vector2 NewCharacterVelocity = (new Vector2(CurrentSelectedTarget.transform.position.x, 0) - new Vector2(transform.position.x, 0)).normalized * 2;
                GetComponent<PlayerMovement>().SetExternalVelocity(NewCharacterVelocity);
            }
        }

        bool CancelCondition = false;
        if (ButtonPresses <= 0)
        {
            if (CurrentSelectedTarget.transform.parent.GetComponent<Enemy>() != null) //hier kommt später die funktion hin die regelt was passiert wenn der spieler gewinnt
                CurrentSelectedTarget.transform.parent.GetComponent<Enemy>().GetHit(transform, 15, 4); //4 evtl auch als parameter hit priority übergeben
            CancelCondition = true;
        }

        CurrentTimeActiveRope += Time.deltaTime / Time.timeScale;
        if (CurrentTimeActiveRope > MaxTimeToWinRopeFight)
        {
            GetComponent<PlayerCombat>().GetHit(CurrentSelectedTarget.transform, 10);
            CancelCondition = true;
        }
        if (CancelCondition)
            CurrentTimeActiveRope = 0;
        return CancelCondition;
    }

    void DeactivateHook()
    {
        CurrentHookState = HookState.Inactive;
        CurrentTargetType = HookType.None;
        CurrentSelectedTarget = null; //vllt brauch ich das gar nicht ? //evlt nur currentselectedpoint == null
        CurrentSwitchTarget = null;
        GetComponent<PlayerMovement>().DisableUserInput(false);
        ResetValues(); //weiß nicht ob das sogut ist?
        //evlt stop all coroutines? --> falls es von einem anderen script her aufgerufen wird
    }

    void CheckTargetType(Collider2D target)
    {
        if (target.CompareTag("HookPoint"))
        {
            if (target.transform.parent != null && target.transform.parent.CompareTag("BigEnemy")) //brauch ihc einen parent null check?
                CurrentTargetType = HookType.BigEnemy;
            else
                CurrentTargetType = HookType.Hook;
        }
        if (target.CompareTag("Throwable"))
            CurrentTargetType = HookType.Throw;
        if (target.CompareTag("RopePoint"))
            CurrentTargetType = HookType.Pull;
    }

    void ResetValues()
    {
        if (RadiusVisualization != null)
            RadiusVisualization.GetComponent<LineRenderer>().enabled = false;

        timeslowTest = 0; //wofür?
        ResetHookPoints();
        Time.timeScale = NormalTimeScale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        CurrentTimeActive = 0;
        Currentvelocity = Vector2.zero;
    }

    void SearchTargetPoint()
    {
        if (CurrentHookState != HookState.Active && CurrentHookState != HookState.SwitchTarget)
            CurrentHookState = HookState.SearchTarget;

        if (RadiusVisualization != null)
        {
            RadiusVisualization.GetComponent<LineRenderer>().enabled = true; //only for radius circle --> remove/change later
            RadiusVisualization.GetComponent<DrawCircle>().radius = HookRadius;
            RadiusVisualization.GetComponent<DrawCircle>().CreatePoints();
        }
        if (UsingController == false)
        {
            if (CurrentHookState == HookState.SearchTarget)
                CurrentSelectedTarget = FindNearestTargetInRange(MouseDirection);
            else if (CurrentSelectedTarget != FindNearestTargetInRange(MouseDirection) && FindNearestTargetInRange(MouseDirection).CompareTag("HookPoint"))
            {
                if (FindNearestTargetInRange(MouseDirection) != null && FindNearestTargetInRange(MouseDirection).CompareTag("HookPoint"))
                {
                    CurrentSwitchTarget = FindNearestTargetInRange(MouseDirection);
                    CurrentHookState = HookState.SwitchTarget;
                }
                else if (FindNearestTargetInRange(MouseDirection) == null)
                {
                    CurrentSwitchTarget = FindNearestTargetInRange(MouseDirection);
                    CurrentHookState = HookState.SwitchTarget;
                }
            }
            VisualizeCone(MouseDirection);
        }
        else
        {
            if (CurrentHookState == HookState.SearchTarget)
                CurrentSelectedTarget = FindNearestTargetInRange(ControllerDirection);
            else if (CurrentSelectedTarget != FindNearestTargetInRange(ControllerDirection))
            {
                if (FindNearestTargetInRange(ControllerDirection) != null && FindNearestTargetInRange(ControllerDirection).CompareTag("HookPoint"))
                {
                    CurrentSwitchTarget = FindNearestTargetInRange(ControllerDirection);
                    CurrentHookState = HookState.SwitchTarget;
                }
                else if (FindNearestTargetInRange(ControllerDirection) == null)
                {
                    CurrentSwitchTarget = FindNearestTargetInRange(ControllerDirection);
                    CurrentHookState = HookState.SwitchTarget;
                }
            }
            VisualizeCone(ControllerDirection);
        }

        SlowTime();
        CurrentTimeActive += Time.deltaTime / Time.timeScale;
        if (CurrentTimeActive > MaxTimeActive)
        {
            ResetValues();
            ActivateHook();
        }
    }

    void ActivateHook() //_direction wahrscheinlich unnötig
    {
        if (CurrentHookState != HookState.Active)
        {
            if (CurrentSwitchTarget != null)
                CurrentSelectedTarget = CurrentSwitchTarget;
            CurrentHookState = HookState.Active;
            if (CurrentSelectedTarget != null) //evtl alle noch vorherigen sachen resetten?
            {
                //Debug.Log("hier"); //resettet alle values wenn auch wenn man nicht switched --> evtl nur if hookstate != active und active erst am ende der funktion setzen
                TargetPosition = CurrentSelectedTarget.transform.position;
                CheckTargetType(CurrentSelectedTarget);
                switch (CurrentTargetType)
                {
                    case HookType.Hook:
                        {
                            ReachedTarget = false; //evtl alles in einer funktion machen lassen für moving hookpoint
                            CurrentTargetPosition = CalculateTargetPoint(transform.position, CurrentSelectedTarget.transform.position, AdditionalTravelDistance);
                            CancelDistance = CalculateCancelDistance(transform.position, CurrentSelectedTarget.transform.position); //beachtet die zusätzliche TravelDistanceNicht
                            GetComponent<PlayerMovement>().DisableUserInput(true);
                            SetVelocityTowardsTarget(CurrentTargetPosition, HookSpeed);
                            FramesTillTarget = CalculateTravelTime(transform.position, CurrentTargetPosition, HookSpeed);
                            break;
                        }
                    case HookType.BigEnemy:
                        {
                            ReachedTarget = false; //evtl alles in einer funktion machen lassen für moving hookpoint
                            CurrentTargetPosition = CurrentSelectedTarget.transform.position;
                            CancelDistance = CalculateCancelDistance(transform.position, CurrentSelectedTarget.transform.position); //beachtet die zusätzliche TravelDistanceNicht
                            GetComponent<PlayerMovement>().DisableUserInput(true);
                            SetVelocityTowardsTarget(CurrentTargetPosition, HookSpeed);
                            FramesTillTarget = CalculateTravelTime(transform.position, CurrentTargetPosition, HookSpeed);
                            break;
                        }
                    case HookType.Pull:
                        {
                            ButtonPresses = NumOfButtonPresses;
                            GetComponent<PlayerMovement>().DisableUserInput(true); // kann evtl nach oben
                            break;
                        }
                    case HookType.Throw:
                        {
                            CurrentSelectedTarget.GetComponent<ThrowableObject>().PickUp(transform, PullSpeed, TargetReachedTolerance);
                            break;
                        }
                }
            }
            else
            {
                CurrentHookState = HookState.Cooldown;
                Vector2 HookDirection = Vector2.zero;
                if (UsingController == false)
                    HookDirection = MouseDirection;
                else
                    HookDirection = ControllerDirection;
                HookCooldown = StartCoroutine(ThrowHook(HookDirection));
            }
        }
    }

    bool HookToEnemy()
    {
        if (Vector2.Distance(transform.position, CurrentSelectedTarget.transform.position) < TargetReachedTolerance)
            ReachedTarget = true;

        if (TargetPosition != (Vector2)CurrentSelectedTarget.transform.position && ReachedTarget == false) //bei big enemy funktioniert das nicht 
        {
            TargetPosition = CurrentSelectedTarget.transform.position;
            CurrentTargetPosition = TargetPosition;
            SetVelocityTowardsTarget(CurrentTargetPosition, HookSpeed);
            FramesTillTarget = CalculateTravelTime(transform.position, CurrentTargetPosition, HookSpeed); //brauch ich das? --> eigentlich schon
        }

        Debug.DrawLine(transform.position, CurrentSelectedTarget.transform.position);
        bool CancelCondition = false;
        if (Vector2.Distance(transform.position, CurrentTargetPosition) < TargetReachedTolerance) //evlt das gleiche wie oben
            CancelCondition = true;

        if (CancelHookWithSpace && (Input.GetButton("Jump") || Input.GetButton("Fire1")) && Vector2.Distance(transform.position, CurrentSelectedTarget.transform.position) < CancelDistance) //falls aktiviert: wenn space gedrückt und bereits ein prozentualer teil des weges erreich wurde
            CancelCondition = true;

        FramesTillTarget -= 1 * Time.timeScale;
        if (FramesTillTarget < 0)
        {
            ReachedTarget = true;
            CancelCondition = true;
        }

        return CancelCondition;
    }

    bool HookToTarget()
    {
        if (Vector2.Distance(transform.position, CurrentSelectedTarget.transform.position) < TargetReachedTolerance)
            ReachedTarget = true;

        if (TargetPosition != (Vector2)CurrentSelectedTarget.transform.position && ReachedTarget == false) //bei big enemy funktioniert das nicht 
        {
            TargetPosition = CurrentSelectedTarget.transform.position;
            CurrentTargetPosition = CalculateTargetPoint(transform.position, TargetPosition, AdditionalTravelDistance);
            SetVelocityTowardsTarget(CurrentTargetPosition, HookSpeed);
            FramesTillTarget = CalculateTravelTime(transform.position, CurrentTargetPosition, HookSpeed); //brauch ich das? --> eigentlich schon
        }

        Debug.DrawLine(transform.position, CurrentSelectedTarget.transform.position);
        bool CancelCondition = false;
        if (Vector2.Distance(transform.position, CurrentTargetPosition) < TargetReachedTolerance) //wenn man sein ziel erreicht hat
            CancelCondition = true;

        if (CancelHookWithSpace && (Input.GetButton("Jump") || Input.GetButton("Fire1")) && Vector2.Distance(transform.position, CurrentSelectedTarget.transform.position) < CancelDistance) //falls aktiviert: wenn space gedrückt und bereits ein prozentualer teil des weges erreich wurde
            CancelCondition = true;

        FramesTillTarget -= 1 * Time.timeScale;
        if (FramesTillTarget < 0)
            CancelCondition = true;

        return CancelCondition;
    }

    void ProgressiveTimeSlowTwo(float x) //muss wahrscheinlihc nichtmal eine coroutine sein
    {
        if (Time.timeScale > MaxTimeSlow)
        {
            Time.timeScale -= Mathf.Pow(x, 2);
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
    }

    void ProgressiveTimeSlow() //muss wahrscheinlihc nichtmal eine coroutine sein
    {
        if (Time.timeScale > MaxTimeSlow)
        {
            Time.timeScale *= 0.93f;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
    }
    IEnumerator ThrowHook(Vector2 _direction)
    {
        int test = 0;
        while (test < HookRadius)
        {
            Vector2 temp = _direction * test;
            Debug.DrawLine(transform.position, (Vector2)transform.position + temp);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, _direction, test, HookPointFilter); //weil player nichtmehr auf dem ignore raycast layer ist
            if (hit.collider != null)
            {
                StartCoroutine(PullBackHook(_direction, test));
                StopCoroutine(HookCooldown);
            }
            test++;
            yield return new WaitForSeconds(0.03f);
        }
        StartCoroutine(PullBackHook(_direction, (int)HookRadius));
    }

    IEnumerator PullBackHook(Vector2 _direction, int _numberOfRepeats)
    {
        for (int i = _numberOfRepeats; i > 0; i--)
        {
            Vector2 temp = _direction * i;
            Debug.DrawLine(transform.position, (Vector2)transform.position + temp, Color.red);
            yield return new WaitForSeconds(0.03f);
        }
        CurrentHookState = HookState.Inactive;
    }

    Vector2 CalculateTargetPoint(Vector2 _start, Vector2 _end, float _additionalDistance) //ändern
    {
        float totalDistance = Vector2.Distance(_start, _end) + AdditionalTravelDistance;
        Vector2 targetPoint = (_end - _start).normalized;
        Vector2 test = _start + targetPoint * totalDistance;
        return test;
    }

    bool CanUseHook()
    {
        if (CancelHookWithNewHook)
            if (CurrentSelectedTarget != null && Vector2.Distance(transform.position, CurrentSelectedTarget.transform.position) < CancelDistance && CurrentTargetType != HookType.Throw)
                return true;
        return false;
    }

    float CalculateTravelTime(Vector2 _startPoint, Vector2 _endPoint, float Speed)
    {
        Vector2 PlayerVelocity = (_endPoint - _startPoint).normalized * Speed;
        float Distance = Vector2.Distance(_startPoint, _endPoint);
        float FramesTillTarget = Distance / (PlayerVelocity.magnitude / 60); //*Time.deltaTime --> funktioniert nur bei vsync aktiviert
        //float Test = Distance / (AdditionalVelocity.magnitude * (Time.deltaTime * 10)); //evtl die richtige lösung/bessere?
        return FramesTillTarget;
    }

    float CalculateCancelDistance(Vector2 _startPoint, Vector2 _endPoint)
    {
        float TotalDistance = Vector2.Distance(_startPoint, _endPoint);
        TotalDistance *= 1 - CancelDistancePercentage;
        return TotalDistance;
    }

    IEnumerator JumpBack() //--> gibt noch bugs 
    {
        int x = 1;
        if (transform.position.x > CurrentSelectedTarget.transform.parent.transform.position.x)
            x = 1;
        else
            x = -1;
        DeactivateHook();
        GetComponent<PlayerMovement>().DisableUserInput(true);
        CurrentHookState = HookState.JumpBack;
        Vector2 JumpBackvelocity = new Vector2(0.5f * x, 0.5f).normalized * HookSpeed; //evtl jump speed
        GetComponent<PlayerMovement>().SetExternalVelocity(JumpBackvelocity);
        yield return new WaitForSeconds(0.4f * 10 / HookSpeed); //bessere lösung finden --> passt fürs erste
        DeactivateHook();
    }

    void SetVelocityTowardsTarget(Vector2 _targetPoint, float _speed) //rename //evlt speed auch als parameter übergeben
    {
        Vector2 NewCharacterVelocity = (_targetPoint - (Vector2)transform.position).normalized * _speed;
        GetComponent<PlayerMovement>().SetExternalVelocity(NewCharacterVelocity);
    }

    Collider2D FindNearestTargetInRange(Vector2 _SearchDirection) //evtl besser als find target oder so --> noch überlegn wie man die vector2.zero geschichte besser lösen könnte //not working? //hier evtl einen kleinen kreis als absicherung bei sehr nahen hookpoints --> cone erst aber einer gewissen distanz
    {
        if (_SearchDirection == Vector2.zero)
            return null;

        Collider2D[] HookPointsInRange = Physics2D.OverlapCircleAll(transform.position, HookRadius, HookPointLayer); //hookPoints in range, .... --> hook points in sight
        for (int i = 0; i < HookPointsInRange.Length; i++)
        {
            if (!TotalHookPoints.Contains(HookPointsInRange[i]))
            {
                TotalHookPoints.Add(HookPointsInRange[i]);
            }
        }
        ResetHookPoints();

        List<Collider2D> HookPointsInSight = new List<Collider2D>();
        for (int i = 0; i < HookPointsInRange.Length; i++)
        {
            float RayCastLenght = Vector2.Distance(transform.position, HookPointsInRange[i].transform.position);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (HookPointsInRange[i].transform.position - transform.position), RayCastLenght, HookPointFilter);
            if (hit == false)
            {
                if (HookPointsInRange[i].CompareTag("Throwable") && HookPointsInRange[i].GetComponent<ThrowableObject>().CurrentObjectState == ThrowableObject.CurrentState.Inactive) //noch keine soute lösung
                    HookPointsInSight.Add(HookPointsInRange[i]);
                else if (!HookPointsInRange[i].CompareTag("Throwable"))
                    HookPointsInSight.Add(HookPointsInRange[i]);
            }
        }

        List<Collider2D> HookPointsInCone = new List<Collider2D>(); //evlt einen besseren namen finden
        for (int i = 0; i < HookPointsInSight.Count; i++) //wie am besten den extra filter applyen?
        {
            VisualizeLine(transform.position, HookPointsInSight[i].transform.position);
            HookPointsInSight[i].GetComponent<SpriteRenderer>().color = Color.red;
            Vector2 PlayerToColliderDirection = (HookPointsInSight[i].transform.position - transform.position).normalized;
            float AngleInDeg = Vector2.Angle(PlayerToColliderDirection, _SearchDirection);

            if (AngleInDeg < Angle || Vector2.Distance(transform.position, HookPointsInSight[i].transform.position) < SafetyRadius)
            {
                HookPointsInCone.Add(HookPointsInSight[i]);
            }
        }

        if (HookPointsInCone.Count == 0)
        {
            return null;
        }

        Collider2D NearestTargetPoint = new Collider2D();
        float LowestAngle = Mathf.Infinity;
        for (int i = 0; i < HookPointsInCone.Count; i++) //bisher kein check ob throwable or not --> siehe old function
        {
            Vector2 PlayerToColliderDirection = (HookPointsInCone[i].transform.position - transform.position).normalized;
            float AngleInDeg = Vector2.Angle(PlayerToColliderDirection, _SearchDirection);
            if (AngleInDeg < LowestAngle)
            {
                if (HookPointsInCone.Count > 1)
                {
                    if (HookPointsInCone[i].CompareTag("Throwable")) //evlt noch ne bessere lösung finden
                    {
                        if (AngleInDeg < PullAngle)
                        {
                            LowestAngle = AngleInDeg;
                            NearestTargetPoint = HookPointsInCone[i];
                        }
                    }
                    else
                    {
                        LowestAngle = AngleInDeg;
                        NearestTargetPoint = HookPointsInCone[i];
                    }
                }
                else
                {
                    LowestAngle = AngleInDeg;
                    NearestTargetPoint = HookPointsInCone[i];
                }
            }
        }

        if (NearestTargetPoint != null)
        {
            NearestTargetPoint.GetComponent<SpriteRenderer>().color = Color.green;
        }
        return NearestTargetPoint;
    }

    void ResetHookPoints() //müssten eigentlich nur hookpoints in TotalHookPoints sein
    {
        for (int i = 0; i < TotalHookPoints.Count; i++)
        {
            TotalHookPoints[i].GetComponent<SpriteRenderer>().color = Color.white;
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

    void VisualizeCone(Vector2 _direction)
    {
        Vector2 DirectionLine = _direction.normalized * HookRadius;
        Vector2 LeftArc = RotateVector(DirectionLine, Angle);
        Vector2 RightArc = RotateVector(DirectionLine, -Angle);
        Vector2 InnerLeftArc = RotateVector(DirectionLine, PullAngle);
        Vector2 InnerRightArc = RotateVector(DirectionLine, -PullAngle);

        Debug.DrawLine(transform.position, (Vector2)transform.position + DirectionLine);
        Debug.DrawLine(transform.position, (Vector2)transform.position + LeftArc);
        Debug.DrawLine(transform.position, (Vector2)transform.position + RightArc);
        Debug.DrawLine(transform.position, (Vector2)transform.position + InnerLeftArc, Color.blue);
        Debug.DrawLine(transform.position, (Vector2)transform.position + InnerRightArc, Color.blue);
    }

    void VisualizeLine(Vector2 _start, Vector2 _end) //eigentlich kein guter name --> weil es nur eine bestimmte linie visualisiert
    {
        Debug.DrawLine(_start, _end);
        float VisualDistance = Vector2.Distance(_start, _end) * CancelDistancePercentage;
        Debug.DrawLine(_start, _start + (_end - _start).normalized * VisualDistance, Color.red);
    }

    void ApplyGravity()
    {
        Currentvelocity += Vector2.up * (-Gravity * Time.deltaTime / Time.timeScale);
        actor.velocity = Currentvelocity;
        actor.velocity = new Vector2(actor.velocity.x, Mathf.Clamp(actor.velocity.y, -MaxFallingSpeed, float.MaxValue));
    }
}




