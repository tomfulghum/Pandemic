using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//requires Component Actor2d
public class ThrowableObject : MonoBehaviour
{
    // Start is called before the first frame update
    public enum CurrentState { Inactive, TravellingToPlayer, PickedUp, Thrown } // getter
    //enum OnImpact
    public CurrentState CurrentObjectState;

    Vector2 CurrentVelocity;
    public float Gravity;
    [Range(1,3)] public float SpeedMultiplier = 1.3f; //später per object typ einstellen
    public bool PickedUp;
    public bool CurrentlyThrown;
    public Transform ObjectToFollow;
    float Speed;
    float TargetReachedTolerance;
    Vector2 _gravity;
    Actor2D actor;
    void Start()
    {
        actor = GetComponent<Actor2D>();
        _gravity = new Vector2(0, Gravity);
    }

    // Update is called once per frame
    void Update()
    {
        switch (CurrentObjectState)
        {
            case CurrentState.TravellingToPlayer:
                {
                    Vector2 objectVelocity = (ObjectToFollow.transform.position - transform.position).normalized * Speed;
                    GetComponent<Actor2D>().velocity = objectVelocity;
                    if (Vector2.Distance(transform.position, ObjectToFollow.transform.position) < TargetReachedTolerance)
                    {
                        CurrentObjectState = CurrentState.PickedUp;
                    }
                    break;
                }
            case CurrentState.PickedUp:
                {
                    transform.position = ObjectToFollow.transform.position;
                    break;
                }
            case CurrentState.Inactive:
                {
                    if (actor.collision.above || actor.collision.below)
                    {
                        actor.velocity = new Vector2(actor.velocity.x, 0);
                    }
                    if (actor.collision.left || actor.collision.right)
                    {
                        actor.velocity = new Vector2(0, actor.velocity.y);
                    }
                    ApplyGravity();
                    break;
                }
            case CurrentState.Thrown:
                {
                    CheckEnemyHit();
                    GetComponent<SpriteRenderer>().color = Color.yellow;
                    if (actor.collision.above || actor.collision.below || actor.collision.left || actor.collision.right)
                    { 
                        CurrentVelocity = Vector2.zero;
                        actor.velocity = CurrentVelocity;
                        CurrentObjectState = CurrentState.Inactive;
                        GetComponent<SpriteRenderer>().color = Color.blue;
                    }
                    ApplyGravity(SpeedMultiplier);
                    break;
                }
        }
    }

    void ApplyGravity(float Multiplier = 1)
    {
        CurrentVelocity += Vector2.up * (-_gravity * Time.deltaTime) * Mathf.Pow(Multiplier,2);
        actor.velocity = CurrentVelocity;
        actor.velocity = new Vector2(actor.velocity.x, Mathf.Clamp(actor.velocity.y, -Gravity, float.MaxValue));
    }

    void CheckEnemyHit()
    {
        Transform enemy = null;
        if (actor.collision.below && actor.collision.below.CompareTag("Enemy"))
            enemy = actor.collision.below;
        if (actor.collision.above && actor.collision.above.CompareTag("Enemy"))
            enemy = actor.collision.above;
        if (actor.collision.left && actor.collision.left.CompareTag("Enemy"))
            enemy = actor.collision.left;
        if (actor.collision.right && actor.collision.right.CompareTag("Enemy"))
            enemy = actor.collision.right;
        if (enemy != null)
        {
            //Debug.Log("hit enemy");
            enemy.GetComponent<Enemy>().GetHit(transform, 10); //besser machen --> direction object zu enemy + knockback force oder so ausrechnen
        }
    }

    public void PickUp(Transform _target, float _speed, float _targetReachedTolerance)
    {
        ObjectToFollow = _target;
        CurrentObjectState = CurrentState.TravellingToPlayer;
        Speed = _speed;
        TargetReachedTolerance = _targetReachedTolerance;
    }

    public void Throw(Vector2 _velocity) // nur ein parameter 
    {
        CurrentVelocity = _velocity * SpeedMultiplier;
        CurrentObjectState = CurrentState.Thrown; 
    }

    public void Drop()
    {
        CurrentObjectState = CurrentState.Inactive;
        ObjectToFollow = null;
        actor.velocity = Vector2.zero;
    }
}
