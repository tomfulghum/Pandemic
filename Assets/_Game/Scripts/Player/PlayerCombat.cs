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
    bool Attacking;
    bool Smashing;
    bool FacingLeft;
    bool HitEnemy; //besser in enemy weil es für jeden enemy einzeln gelten muss
    Vector3 lastPosition;
    float xAxis;
    // Start is called before the first frame update
    void Start()
    {
        lastPosition = transform.position;
        xAxis = Input.GetAxis("Horizontal");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire1") && Attacking == false)
        {
            SetFacingDirection();
            StartCoroutine(Attack());
        }
        if(Attacking)
        {
            VisualizeAttack();
            CheckEnemyHit();
        }
        if (Input.GetButtonDown("Fire1") && Input.GetAxis("Vertical") == -1)
        {
            if(!Smashing)
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
                    hit.collider.GetComponent<Enemy>().GetHit();
                    //Debug.Log("i hit an enemy");
                }
            }
        }
        lastPosition = transform.position;
    }

    void SetFacingDirection()
    {
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
    }

    void CheckEnemyHit()
    {
        Collider2D[] ColliderInRange = Physics2D.OverlapCircleAll(transform.position, AttackRange);
        for(int i = 0; i < ColliderInRange.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (ColliderInRange[i].transform.position - transform.position), AttackRange, layerMask);
            if(hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                Vector2 PlayerToCollider = (ColliderInRange[i].transform.position - transform.position).normalized;
                Vector2 Direction;
                if (FacingLeft)
                {
                    Direction = Vector2.left;
                }
                else
                {
                    Direction = Vector2.right;
                }
                float angleInDeg = Vector2.Angle(PlayerToCollider, Direction);
                if(angleInDeg < AttackAngle)
                {
                    if (HitEnemy == false)
                    {
                        hit.collider.GetComponent<Enemy>().GetHit();
                    }
                    HitEnemy = true;
                }
            }
        }

    }

    IEnumerator Attack() //darauf achten während dem air attack etwas gravity dazuzurechnen
    {
        Attacking = true;
        if(AllowMovingWhileAttacking)
        {
            Vector3 currentVelocity = GetComponent<Rigidbody2D>().velocity.normalized * 2;
            GetComponent<PlayerMovement>().DisableUserInput(true);
            GetComponent<PlayerMovement>().SetExternalVelocity(currentVelocity);
        }
        else
        {
            GetComponent<PlayerMovement>().DisableUserInput(true);
        }
        yield return new WaitForSeconds(0.5f);
        GetComponent<PlayerMovement>().DisableUserInput(false);
        Attacking = false;
        HitEnemy = false;
    }

    void VisualizeAttack()
    {
        Vector2 DirectionLine;
        if (FacingLeft)
        {
            DirectionLine = Vector2.left * AttackRange;
        }
        else
        {
            DirectionLine = Vector2.right * AttackRange;
        }
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