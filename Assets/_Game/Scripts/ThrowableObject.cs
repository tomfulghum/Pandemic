using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//requires Component Actor2d
public class ThrowableObject : MonoBehaviour
{
    // Start is called before the first frame update
    public enum CurrentState { Inactive, TravellingToPlayer, PickedUp, Thrown } // getter
    //enum OnImpact
    [HideInInspector] public CurrentState CurrentObjectState = CurrentState.Inactive;

    Vector2 CurrentVelocity;
    public float Gravity = 10f;
    [Range(1,3)] public float SpeedMultiplier = 1.4f; //später per object typ einstellen

    [HideInInspector] public Transform ObjectToFollow;
    float Speed;
    float TargetReachedTolerance;
    Vector2 _gravity;
    Actor2D actor;
    Rigidbody2D m_rb;

    void Start()
    {
        actor = GetComponent<Actor2D>();
        m_rb = GetComponent<Rigidbody2D>();
        _gravity = new Vector2(0, Gravity);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (CurrentObjectState)
        {
            case CurrentState.TravellingToPlayer:
                {
                    Vector2 objectVelocity = (ObjectToFollow.transform.position - transform.position).normalized * Speed;
                    m_rb.velocity = objectVelocity;
                    if (Vector2.Distance(transform.position, ObjectToFollow.transform.position) < TargetReachedTolerance)
                    {
                        CurrentObjectState = CurrentState.PickedUp;
                    }
                    break;
                }
            case CurrentState.PickedUp:
                {
                    m_rb.MovePosition(ObjectToFollow.GetComponent<Rigidbody2D>().position);
                    break;
                }
            case CurrentState.Inactive:
                {
                    //if (actor.contacts.above || actor.contacts.below)
                    //{
                    //    actor.velocity = new Vector2(actor.velocity.x, 0);
                    //}
                    //if (actor.contacts.left || actor.contacts.right)
                    //{
                    //    actor.velocity = new Vector2(0, actor.velocity.y);
                    //}
                    //ApplyGravity();
                    break;
                }
            case CurrentState.Thrown:
                {
                    CheckEnemyHit();
                    GetComponent<SpriteRenderer>().color = Color.yellow;
                    if (actor.contacts.above || actor.contacts.below || actor.contacts.left || actor.contacts.right)
                    { 
                        CurrentVelocity = Vector2.zero;
                        m_rb.velocity = CurrentVelocity;
                        CurrentObjectState = CurrentState.Inactive;
                        GetComponent<SpriteRenderer>().color = Color.blue;
                    }
                    //ApplyGravity(SpeedMultiplier);
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
        if (actor.contacts.below && actor.contacts.below.CompareTag("Enemy"))
            enemy = actor.contacts.below;
        if (actor.contacts.above && actor.contacts.above.CompareTag("Enemy"))
            enemy = actor.contacts.above;
        if (actor.contacts.left && actor.contacts.left.CompareTag("Enemy"))
            enemy = actor.contacts.left;
        if (actor.contacts.right && actor.contacts.right.CompareTag("Enemy"))
            enemy = actor.contacts.right;
        if (enemy != null)
            enemy.GetComponent<Enemy>().GetHit(transform, 25, 4); //besser machen --> direction object zu enemy + knockback force oder so ausrechnen //4 auch als parameter hit priority übergeben
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
        m_rb.velocity = _velocity * SpeedMultiplier;
        CurrentObjectState = CurrentState.Thrown; 
    }

    public void Drop()
    {
        CurrentObjectState = CurrentState.Inactive;
        ObjectToFollow = null;
        actor.velocity = Vector2.zero;
    }
}
