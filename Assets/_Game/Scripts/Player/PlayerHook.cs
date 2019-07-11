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
public class PlayerHook : MonoBehaviour
{
    public enum ControllType { Keyboard, Controller }
    public enum ControllStick { Left, Right }
    public enum TimeSlow { NoSlow, Instant, SlowFast, FastSlow }

    public ControllType controlls;
    public ControllStick stick;
    public float HookRadius;
    public float Angle;
    public float HookSpeed;
    public float MaxTimeSlow;
    public float MaxTimeActive;
    public float TargetReachedTolerance; //wie nah muss man am ziel sein damit der hook abbricht
    public bool CancelHookWithSpace;
    public bool CancelHookWithNewHook;
    public float CancelDistancePercentage; //wie viel prozent des abstands der spieler geschafft haben muss bevor er den hook abbrechen kann
    public float AdditionalTravelDistance; //in percent
    public bool UseCancelThroughTravelTime;
    //public bool UseTimeSlow;
    public TimeSlow FormOfTimeSlow;
    public GameObject RadiusVisualization; //rename
    public LayerMask layer_mask;

    
    List<Collider2D> TotalHookPoints;

    bool HookActive;
    float NormalTimeScale;
    float CancelDistance;
    float ActiveTime;
    bool HookActivated;
    float FramesTillTarget;
    bool AdditionalTravelTest;
    bool PullBackActive;
    bool PullToBigEnemy;

    float timeslowTest;
    float currentTimeActive;

    Collider2D TargetHookPoint;
    Collider2D CurrentSelectedPoint;

    Vector2 CurrentJoystickDirection;
    Vector2 TargetPoint;
    Vector2 TargetPosition;

    Coroutine HookCooldown = null;

    // Start is called before the first frame update
    void Start()
    {
        NormalTimeScale = Time.timeScale;
        TotalHookPoints = new List<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 ControllerDirection = Vector2.zero;
        if (stick == ControllStick.Left)
        {
            ControllerDirection.x = Input.GetAxis("Horizontal");
            ControllerDirection.y = Input.GetAxis("Vertical");
            ControllerDirection = ControllerDirection.normalized;
        }
        if (stick == ControllStick.Right)
        {
            ControllerDirection.x = Input.GetAxis("RightHorizontal");
            ControllerDirection.y = Input.GetAxis("RightVertical");
            ControllerDirection = ControllerDirection.normalized;
        }
        //Debug.Log(Input.GetAxis("ControllerHook"));
        if ((HookActive == false || CanUseHook()) && PullBackActive == false) //vllt hook erst auf nicht active wetzen wenn ziel erreicht ist &&hookNotThrown
        {
            if (Input.GetButton("Hook") || Input.GetAxis("ControllerHook") == 1) // && time variable < max time active --> max time active macht am meisten sinn wenn es einen cooldown gibt
            {
                HookActivated = true;
                if (RadiusVisualization != null)
                {
                    RadiusVisualization.GetComponent<LineRenderer>().enabled = true; //only for radius circle --> remove/change later
                    RadiusVisualization.GetComponent<DrawCircle>().radius = HookRadius;
                    RadiusVisualization.GetComponent<DrawCircle>().CreatePoints();
                }
                if (controlls == ControllType.Keyboard)
                {
                    Vector2 MousePositionForVisualization = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 DirectionLine = (MousePositionForVisualization - (Vector2)transform.position).normalized;
                    CurrentSelectedPoint = FindTargetHookPoint(DirectionLine);
                    Visualize();
                }
                else
                {
                    CurrentSelectedPoint = FindTargetHookPoint(ControllerDirection);
                    VisualizeForController(ControllerDirection);
                }
                switch(FormOfTimeSlow)
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
                currentTimeActive += Time.deltaTime / Time.timeScale;
               // Debug.Log(currentTimeActive);
                if (currentTimeActive > MaxTimeActive)
                {
                    ActivateHook(ControllerDirection);
                }
            }
            else if ((Input.GetButtonUp("Hook") || Input.GetAxis("ControllerHook") == 0) && HookActivated == true) //für controller ist das blöd //evtl nicht == 0 sondern ungleich 1
            {
                ActivateHook(ControllerDirection);
            }
        }

        if (HookActive)
        {
            if (Vector2.Distance(transform.position, TargetPosition) < TargetReachedTolerance)
            {
                AdditionalTravelTest = false;
            }
            if (TargetPosition != (Vector2)TargetHookPoint.transform.position && AdditionalTravelTest) //bei big enemy funktioniert das nicht
            {
                /*
                TargetPosition = TargetHookPoint.transform.position;
                MoveTowardsHookPoint(TargetPosition);
                */
                TargetPosition = TargetHookPoint.transform.position;
                TargetPoint = CalculateTargetPoint(transform.position, TargetPosition, AdditionalTravelDistance);
                MoveTowardsHookPoint(TargetPoint);
                FramesTillTarget = CalculateTravelTime(Vector2.Distance(transform.position, TargetPoint), HookSpeed); //brauch ich das? --> eigentlich schon
            }
            FramesTillTarget -= 1 * Time.timeScale;
            // Debug.Log(FramesTillTarget);
            Debug.DrawLine(transform.position, TargetPosition);
            bool CancelCondition = false;
            if (Vector2.Distance(transform.position, TargetPoint) < TargetReachedTolerance) //wenn man sein ziel erreicht hat
            {
                CancelCondition = true;
            }

            if (CancelHookWithSpace && Input.GetKey("space") && Vector2.Distance(transform.position, TargetHookPoint.transform.position) < CancelDistance) //falls aktiviert: wenn space gedrückt und bereits ein prozentualer teil des weges erreich wurde
            {
                CancelCondition = true;
            }

            if (UseCancelThroughTravelTime && FramesTillTarget < 0)
            {
                CancelCondition = true;
            }

            if (CancelCondition)
            {
                // Invoke("DeactivatePullToTarget", 0.2f);
                //HookActive = false;
                DeactivatePullToTarget();
                //additionalTravel;
            }
        }
    }

    void ActivateHook(Vector2 _direction)
    {
        timeslowTest = 0;
        HookActivated = false; //wofür?
        ResetHookPoints();
        if (RadiusVisualization != null)
        {
            RadiusVisualization.GetComponent<LineRenderer>().enabled = false;
        }
        Time.timeScale = NormalTimeScale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        //Debug.Log(CurrentSelectedPoint);
        if (CurrentSelectedPoint != null)
        {
            AdditionalTravelTest = true;
            HookActive = true;
            TargetHookPoint = CurrentSelectedPoint;
            TargetPosition = TargetHookPoint.transform.position;
            CancelDistance = CalculateCancelDistance(); //distanz vom spieler bis zum target (beachtet die extra distanz nicht)
            if (TargetHookPoint.transform.parent != null && TargetHookPoint.transform.parent.CompareTag("BigEnemy"))
            {
                PullToBigEnemy = true;
                TargetPoint = TargetPosition;
            }
            else
            {
                PullToBigEnemy = false;
                TargetPoint = CalculateTargetPoint(transform.position, TargetPosition, AdditionalTravelDistance);
            }

            MoveTowardsHookPoint(TargetPoint);
            FramesTillTarget = CalculateTravelTime(Vector2.Distance(transform.position, TargetPoint), HookSpeed);
            /*
            MoveTowardsHookPoint(TargetPosition);
            FramesTillTarget = CalculateTravelTime(Vector2.Distance(transform.position, TargetPosition), HookSpeed);
            */
            currentTimeActive = 0;
        }
        else if (TargetHookPoint == null)
        {
            if (controlls == ControllType.Keyboard)
            {
                Vector2 MousePositionForVisualization = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 DirectionLine = (MousePositionForVisualization - (Vector2)transform.position).normalized;
                HookCooldown = StartCoroutine(throwHook(DirectionLine));
            }
            if (controlls == ControllType.Controller)
            {
                HookCooldown = StartCoroutine(throwHook(_direction));
            }
        }
    }

    void ProgressiveTimeSlowTwo(float x) //muss wahrscheinlihc nichtmal eine coroutine sein
    {
        if (Time.timeScale > MaxTimeSlow)
        {
            Time.timeScale -= Mathf.Pow(x,2);
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
    IEnumerator throwHook(Vector2 _direction)
    {
        PullBackActive = true;
        //Vector2 MousePositionForVisualization = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector2 DirectionLine = (MousePositionForVisualization - (Vector2)transform.position).normalized;

        int test = 0;
        while (test < HookRadius)
        {
            Vector2 temp = _direction * test;
            Debug.DrawLine(transform.position, (Vector2)transform.position + temp);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, _direction, test, layer_mask); //weil player nichtmehr auf dem ignore raycast layer ist
            if (hit.collider != null)
            {
                //Debug.Log("i hit smth");
                StartCoroutine(pullBackHook(_direction, test));
                StopCoroutine(HookCooldown);
            }
            test++;
            yield return new WaitForSeconds(0.03f);
        }
        StartCoroutine(pullBackHook(_direction, (int)HookRadius));
    }

    IEnumerator pullBackHook(Vector2 _direction, int _numberOfRepeats)
    {
        for (int i = _numberOfRepeats; i > 0; i--)
        {
            Vector2 temp = _direction * i;
            Debug.DrawLine(transform.position, (Vector2)transform.position + temp, Color.red);
            yield return new WaitForSeconds(0.03f);
        }
        PullBackActive = false;
        currentTimeActive = 0;
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
        {
            if (TargetHookPoint != null && Vector2.Distance(transform.position, TargetHookPoint.transform.position) < CancelDistance) //TargetHookPoint != null why?
            {
                return true;
            }
        }
        return false;
    }

    float CalculateTravelTime(float Distance, float Speed)
    {
        Vector2 AdditionalVelocity = ((Vector2)TargetHookPoint.transform.position - (Vector2)transform.position).normalized * HookSpeed;
        //Debug.Log(Distance);
        //Debug.Log(AdditionalVelocity.magnitude);
        float FramesTillTarget = Distance / (AdditionalVelocity.magnitude / 60);//*Time.deltaTime
        float Test = Distance / (AdditionalVelocity.magnitude * (Time.deltaTime * 10)); //evtl die richtige lösung/bessere?
        //Debug.Log(FramesTillTarget);
        //Debug.Log(Test);
        return FramesTillTarget;
    }

    float CalculateCancelDistance()
    {
        float TotalDistance = Vector2.Distance(transform.position, TargetHookPoint.transform.position);
        TotalDistance *= 1 - CancelDistancePercentage;
        return TotalDistance;
    }

    void DeactivatePullToTarget() //just for testing
    {
        if (PullToBigEnemy == true)
        {
            StartCoroutine(JumpBack());
        }
        else
        {
            GetComponent<PlayerMovement>().DisableUserInput(false);
        }
        HookActive = false; //evtl woanders besser
        TargetHookPoint = null;
        Physics2D.IgnoreLayerCollision(10, 11, false);
    }

    IEnumerator JumpBack()
    {
        int x = 1;
        if(transform.position.x > TargetHookPoint.transform.parent.transform.position.x)
        {
            x = 1;
        }
        else
        {
            x = -1;
        }
        Vector2 JumpBackvelocity = new Vector2(0.5f*x, 0.5f).normalized * HookSpeed; //evtl jump speed
        GetComponent<PlayerMovement>().SetExternalVelocity(JumpBackvelocity);
        yield return new WaitForSeconds(0.4f); //bessere lösung finden
        GetComponent<PlayerMovement>().DisableUserInput(false);
    }

    void MoveTowardsHookPoint(Vector2 _targetPoint) //rename
    {
        Vector2 NewCharacterVelocity = (_targetPoint - (Vector2)transform.position).normalized * HookSpeed;
        GetComponent<PlayerMovement>().DisableUserInput(true);
        GetComponent<PlayerMovement>().SetExternalVelocity(NewCharacterVelocity);
        Physics2D.IgnoreLayerCollision(10, 11, true);
    }

    Collider2D FindTargetHookPoint(Vector2 _direction)
    {
        Collider2D[] ColliderInRange = Physics2D.OverlapCircleAll(transform.position, HookRadius);
        Collider2D NearestHookPoint = new Collider2D();
        Vector3 MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float LowestAngle = Mathf.Infinity;

        for (int i = 0; i < ColliderInRange.Length; i++)
        {
            if (!TotalHookPoints.Contains(ColliderInRange[i]))
            {
                TotalHookPoints.Add(ColliderInRange[i]);
            }
        }
        ResetHookPoints();
        for (int i = 0; i < ColliderInRange.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (ColliderInRange[i].transform.position - transform.position), HookRadius, layer_mask);
            if (hit.collider != null && hit.collider.CompareTag("HookPoint"))
            {
                VisualizeLines(transform.position, hit.collider.transform.position);
                hit.collider.GetComponent<SpriteRenderer>().color = Color.red;
                Vector2 PlayerToCollider = (ColliderInRange[i].transform.position - transform.position).normalized;
                //Vector2 PlayerToMouse = (MousePosition - transform.position).normalized;

                float angleInDeg = Vector2.Angle(PlayerToCollider, _direction);
                if (angleInDeg < Angle && angleInDeg < LowestAngle)
                {
                    LowestAngle = angleInDeg;
                    NearestHookPoint = hit.collider;
                }
            }
        }
        if (NearestHookPoint != null)
        {
            NearestHookPoint.GetComponent<SpriteRenderer>().color = Color.green;
        }
        return NearestHookPoint;
    }

    void ResetHookPoints() //resets only visualization
    {
        for (int i = 0; i < TotalHookPoints.Count; i++)
        {
            if (TotalHookPoints[i].CompareTag("HookPoint"))
            {
                TotalHookPoints[i].GetComponent<SpriteRenderer>().color = Color.white;
            }
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

    void Visualize()
    {
        Vector2 MousePositionForVisualization = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 DirectionLine = (MousePositionForVisualization - (Vector2)transform.position).normalized * HookRadius;
        Vector2 LeftArc = RotateVector(DirectionLine, Angle);
        Vector2 RightArc = RotateVector(DirectionLine, -Angle);

        Debug.DrawLine(transform.position, (Vector2)transform.position + DirectionLine);
        Debug.DrawLine(transform.position, (Vector2)transform.position + LeftArc);
        Debug.DrawLine(transform.position, (Vector2)transform.position + RightArc);
    }

    void VisualizeForController(Vector2 _direction)
    {
        Vector2 DirectionLine = _direction * HookRadius;
        Vector2 LeftArc = RotateVector(DirectionLine, Angle);
        Vector2 RightArc = RotateVector(DirectionLine, -Angle);

        Debug.DrawLine(transform.position, (Vector2)transform.position + DirectionLine);
        Debug.DrawLine(transform.position, (Vector2)transform.position + LeftArc);
        Debug.DrawLine(transform.position, (Vector2)transform.position + RightArc);
    }

    void VisualizeLines(Vector2 _start, Vector2 _end)
    {
        Debug.DrawLine(_start, _end);
        float VisualDistance = Vector2.Distance(_start, _end) * CancelDistancePercentage;
        Debug.DrawLine(_start, _start + (_end - _start).normalized * VisualDistance, Color.red);
    }

    //time slow ramp up/down
}




