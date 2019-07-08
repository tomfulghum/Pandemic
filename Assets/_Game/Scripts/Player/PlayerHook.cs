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
public class PlayerHook : MonoBehaviour
{
    public enum ControllType { Keyboard, Controller }

    public ControllType controlls;
    public float HookRadius;
    public float Angle;
    public float MaxTimeSlow;
    public float MaxActiveTime;
    public float HookSpeed;
    public float HookCancelDistance; //wie nah muss man am ziel sein damit der hook abbricht
    public bool CancelHookWithSpace;
    public bool CancelHookWithNewHook;
    public float CancelDistancePercentage; //wie viel prozent des abstands der spieler geschafft haben muss bevor er den hook abbrechen kann
    public float AdditionalTravelDistance; //in percent
    public bool UseCancelThroughTravelTime;
    public bool VisualizeLines;
    public bool InterpolateBetweenColors;
    public Color FirstColor;
    public Color SecondColor;
    public GameObject RadiusVisualization; //rename

    List<Collider2D> TotalHookPoints;

    bool HookActive;
    float NormalTimeScale;
    float CancelDistance;
    float ActiveTime;
    bool HookActivated;
    float FramesTillTarget;


    Collider2D TargetHookPoint;
    Collider2D CurrentSelectedPoint;

    Vector2 CurrentJoystickDirection;
    Vector2 TargetPoint;
    Vector2 TargetPosition;
    Vector3 MousePositionForVisualization;
    // Start is called before the first frame update
    void Start()
    {
        MaxActiveTime *= 0.1f;
        NormalTimeScale = Time.timeScale;
        TotalHookPoints = new List<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Input.GetAxis("ControllerHook"));
        if (!HookActive || CancelHookThroughNewHook()) //vllt hook erst auf nicht active wetzen wenn ziel erreicht ist
        {
            if (Input.GetButton("Hook") && ActiveTime < MaxActiveTime || Input.GetAxis("ControllerHook") == 1 && ActiveTime < MaxActiveTime) // && time variable < max time active --> max time active macht am meisten sinn wenn es einen cooldown gibt
            {
                HookActivated = true;
                ActiveTime += Time.deltaTime;
                if (VisualizeLines)
                {
                    if (RadiusVisualization != null)
                    {
                        RadiusVisualization.GetComponent<LineRenderer>().enabled = true; //only for radius circle --> remove/change later
                        RadiusVisualization.GetComponent<DrawCircle>().radius = HookRadius;
                        RadiusVisualization.GetComponent<DrawCircle>().CreatePoints();
                    }
                    if (controlls == ControllType.Keyboard)
                    {
                        Visualize();
                    }
                    else
                    {
                        VisualizeForController();
                    }
                }
                if (controlls == ControllType.Keyboard)
                {
                    CurrentSelectedPoint = FindTargetHookPoint();
                }
                else
                {
                    CurrentSelectedPoint = FindTargetHookPointForController();
                }
                Time.timeScale = MaxTimeSlow;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
            else if (Input.GetButtonUp("Hook") || ActiveTime > MaxActiveTime || Input.GetAxis("ControllerHook") == 0 && HookActivated == true) //für controller ist das blöd
            {
                HookActivated = false;
                ActiveTime = 0;
                ResetHookPoints();
                if (RadiusVisualization != null)
                {
                    RadiusVisualization.GetComponent<LineRenderer>().enabled = false;
                }
                Time.timeScale = NormalTimeScale;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
                if (CurrentSelectedPoint != null)
                {
                    HookActive = true;
                    TargetHookPoint = CurrentSelectedPoint;
                    TargetPosition = TargetHookPoint.transform.position;
                    CancelDistance = CalculateCancelDistance(); //distanz vom spieler bis zum target (beachtet die extra distanz nicht)
                    //MoveTowardsHookPoint(TargetHookPoint);
                }
                if(TargetHookPoint != null)
                {
                    TargetPoint = CalculateTargetPoint(transform.position, TargetHookPoint.transform.position, AdditionalTravelDistance); //evlt muss gar nicht mehrfach berechnet werden
                    MoveTowardsHookPoint(TargetPoint);
                    FramesTillTarget = CalculateTravelTime(Vector2.Distance(transform.position, TargetPoint),HookSpeed);
                    //Debug.Log(FramesTillTarget);
                }
            }
        }

        if (HookActive)
        {
            FramesTillTarget -= HookSpeed * Time.timeScale;
            //Debug.Log(FramesTillTarget);
            Debug.DrawLine(transform.position, TargetPoint);
            bool CancelCondition = false;
            if (Vector2.Distance(transform.position, TargetPoint) < HookCancelDistance) //wenn man sein ziel erreicht hat
            {
                CancelCondition = true;
            }
           
            if (CancelHookWithSpace && Input.GetKey("space") && Vector2.Distance(transform.position, TargetHookPoint.transform.position) < CancelDistance) //falls aktiviert: wenn space gedrückt und bereits ein prozentualer teil des weges erreich wurde
            {
                CancelCondition = true;
            }
           
            if(UseCancelThroughTravelTime && FramesTillTarget < 0)
            {
                CancelCondition = true; 
            }
            if (CancelCondition)
            {
                DeactivatePullToTarget();
            }
            if(TargetPosition != (Vector2)TargetHookPoint.transform.position)
            {
                TargetPosition = (Vector2)TargetHookPoint.transform.position;
                Vector2 test = CalculateTargetPoint(transform.position, TargetHookPoint.transform.position, AdditionalTravelDistance); //evlt muss gar nicht mehrfach berechnet werden
                MoveTowardsHookPoint(test);
            }
        }
    }

    public bool CheckTargetHit()
    {
        if (TargetHookPoint != null)
        {
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.6f, GetComponent<Rigidbody2D>().velocity, 6f);
           // Debug.DrawLine(transform.position, GetComponent<Rigidbody2D>().velocity.normalized*2f, Color.magenta);
            //Debug.Log(hit.collider);
            if(hit.collider == TargetHookPoint || hit.collider == null)
            {
                return false;
            } else
            {
                return true;
            }
        }
        return true;
    }

    public float CalculateTravelTime(float Distance, float Speed)
    {
        Vector2 AdditionalVelocity = ((Vector2)TargetHookPoint.transform.position - (Vector2)transform.position).normalized * HookSpeed;
        //Debug.Log(Distance);
        //Debug.Log(AdditionalVelocity.magnitude);
        float FramesTillTarget = Distance /(AdditionalVelocity.magnitude * Time.deltaTime);
        return FramesTillTarget;
    }

    public Vector2 CalculateTargetPoint(Vector2 _start, Vector2 _end, float _additionalDistance)
    {
        float totalDistance = Vector2.Distance(_start, _end) + AdditionalTravelDistance;
        Vector2 targetPoint = (_end - _start).normalized;
        Vector2 test = _start + targetPoint * totalDistance;
        return test;
    }

    public bool CancelHookThroughNewHook() //rename
    {
        if(CancelHookWithNewHook)
        {
            if(TargetHookPoint != null && Vector2.Distance(transform.position, TargetHookPoint.transform.position) < CancelDistance)
            {
                return true;
            }
        }
        return false;
    }

    public void Visualize()
    {
        MousePositionForVisualization = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 DirectionLine = ((Vector2)MousePositionForVisualization - (Vector2)transform.position).normalized * HookRadius;
        Vector2 LeftArc = RotateVector(DirectionLine, Angle);
        Vector2 RightArc = RotateVector(DirectionLine, -Angle);

        Debug.DrawLine(transform.position, (Vector2)transform.position + DirectionLine);
        Debug.DrawLine(transform.position, (Vector2)transform.position + LeftArc);
        Debug.DrawLine(transform.position, (Vector2)transform.position + RightArc);
    }

    public void VisualizeForController()
    {
        Vector2 Direction;
        //Vector2 test = Vector2.zero;
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Direction.x = x;
        Direction.y = y;
       // if (Direction != CurrentJoystickDirection)
        //{
        //    Debug.Log("hier");
        //     test = Vector2.Lerp(Direction, CurrentJoystickDirection, 0.000001f*Time.deltaTime);
       // } 
        MousePositionForVisualization = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector2 DirectionLine = ((Vector2)MousePositionForVisualization - (Vector2)transform.position).normalized * HookRadius;
        //Vector2 DirectionLine = Direction.normalized * HookRadius;
        //Vector2 DirectionLine = CurrentJoystickDirection.normalized * HookRadius;
        Vector2 DirectionLine = Direction.normalized * HookRadius;
        //Vector2 DirectionLine = test.normalized * HookRadius;
        Vector2 LeftArc = RotateVector(DirectionLine, Angle);
        Vector2 RightArc = RotateVector(DirectionLine, -Angle);

        Debug.DrawLine(transform.position, (Vector2)transform.position + DirectionLine);
        Debug.DrawLine(transform.position, (Vector2)transform.position + LeftArc);
        Debug.DrawLine(transform.position, (Vector2)transform.position + RightArc);
    }

    public float CalculateCancelDistance()
    {
        float TotalDistance = Vector2.Distance(transform.position, TargetHookPoint.transform.position);
        TotalDistance *= 1 - CancelDistancePercentage;
        return TotalDistance;
    }

    public void DeactivatePullToTarget() //just for testing
    {
        HookActive = false;
        GetComponent<PlayerMovement>().DisableUserInput(false);
        TargetHookPoint = null;
    }

    public void MoveTowardsHookPoint(Collider2D _target) //rename
    {
        Vector2 AdditionalVelocity = ((Vector2)_target.transform.position - (Vector2)transform.position).normalized * HookSpeed;
        GetComponent<PlayerMovement>().DisableUserInput(true);
        GetComponent<PlayerMovement>().SetExternalVelocity(AdditionalVelocity);
    }

    public void MoveTowardsHookPoint(Vector2 _target) //rename
    {
        Vector2 AdditionalVelocity = (_target - (Vector2)transform.position).normalized * HookSpeed;
        GetComponent<PlayerMovement>().DisableUserInput(true);
        GetComponent<PlayerMovement>().SetExternalVelocity(AdditionalVelocity);
    }

    public Vector2 RotateVector(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }


    Collider2D FindTargetHookPoint() //return gameObject/Collider //nur zum test ColliderZurückgeben
    {
        Collider2D[] HookPointsInRange = Physics2D.OverlapCircleAll(transform.position, HookRadius);
        Collider2D NearestHookPoint = new Collider2D();
        Vector3 MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float LowestAngle = Mathf.Infinity;
        for (int i = 0; i < TotalHookPoints.Count; i++)
        {
            if (TotalHookPoints[i].CompareTag("HookPoint"))
            {
                TotalHookPoints[i].GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
        for (int i = 0; i < HookPointsInRange.Length; i++)
        {
            if (!TotalHookPoints.Contains(HookPointsInRange[i]))
            {
                TotalHookPoints.Add(HookPointsInRange[i]);
            }
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (HookPointsInRange[i].transform.position - transform.position), HookRadius);
            if (hit.collider != null && hit.collider.CompareTag("HookPoint"))
            {
                Debug.DrawLine(transform.position, hit.collider.transform.position);
                float VisualDistance = Vector2.Distance(transform.position, hit.collider.transform.position) * CancelDistancePercentage;
                Debug.DrawLine(transform.position, transform.position + (hit.collider.transform.position - transform.position).normalized * VisualDistance, Color.red);
                hit.collider.GetComponent<SpriteRenderer>().color = Color.red;
                Vector2 PlayerToCollider = (HookPointsInRange[i].transform.position - transform.position).normalized;
                Vector2 PlayerToMouse = (MousePosition - transform.position).normalized;
                float angleInDeg = Vector2.Angle(PlayerToCollider, PlayerToMouse);
                if (InterpolateBetweenColors)
                {
                    float test = 1 - (angleInDeg / 360 * 2);
                    //Debug.Log(test);
                    hit.collider.GetComponent<SpriteRenderer>().color = Color.Lerp(FirstColor, SecondColor, test);
                }
                if (angleInDeg < Angle)
                {
                    if (angleInDeg < LowestAngle)
                    {
                        LowestAngle = angleInDeg;
                        NearestHookPoint = hit.collider;
                    }
                }
            }
        }
        if (NearestHookPoint != null)
        {
            NearestHookPoint.GetComponent<SpriteRenderer>().color = Color.green;
            Debug.Log(Vector2.Distance(transform.position, NearestHookPoint.transform.position));
        }
        return NearestHookPoint;
    }

    Collider2D FindTargetHookPointForController() //return gameObject/Collider //nur zum test ColliderZurückgeben
    {
        Collider2D[] HookPointsInRange = Physics2D.OverlapCircleAll(transform.position, HookRadius);
        Collider2D NearestHookPoint = new Collider2D();
        Vector2 Direction;
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Direction.x = x;
        Direction.y = y;
        Direction = Direction.normalized;
        float LowestAngle = Mathf.Infinity;
        for (int i = 0; i < TotalHookPoints.Count; i++)
        {
            if (TotalHookPoints[i].CompareTag("HookPoint"))
            {
                TotalHookPoints[i].GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
        for (int i = 0; i < HookPointsInRange.Length; i++)
        {
            if (!TotalHookPoints.Contains(HookPointsInRange[i]))
            {
                TotalHookPoints.Add(HookPointsInRange[i]);
            }
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (HookPointsInRange[i].transform.position - transform.position), HookRadius);
            if (hit.collider != null && hit.collider.CompareTag("HookPoint"))
            {
                Debug.DrawLine(transform.position, hit.collider.transform.position);
                float VisualDistance = Vector2.Distance(transform.position, hit.collider.transform.position) * CancelDistancePercentage;
                Debug.DrawLine(transform.position, transform.position + (hit.collider.transform.position - transform.position).normalized * VisualDistance, Color.red);
                hit.collider.GetComponent<SpriteRenderer>().color = Color.red;
                Vector2 PlayerToCollider = (HookPointsInRange[i].transform.position - transform.position).normalized;
                float angleInDeg = Vector2.Angle(PlayerToCollider, Direction);

                if (InterpolateBetweenColors)
                {
                    float test = 1 - (angleInDeg / 360 * 2);
                    //Debug.Log(test);
                    hit.collider.GetComponent<SpriteRenderer>().color = Color.Lerp(FirstColor, SecondColor, test);
                }
                if (angleInDeg < Angle)
                {
                    if (angleInDeg < LowestAngle)
                    {
                        LowestAngle = angleInDeg;
                        NearestHookPoint = hit.collider;
                    }
                }
            }
        }
        if (NearestHookPoint != null)
        {
            NearestHookPoint.GetComponent<SpriteRenderer>().color = Color.green;
            //Debug.Log(Vector2.Distance(transform.position, NearestHookPoint.transform.position));
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
    //time slow ramp up/down
}




