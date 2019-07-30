using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//requires Component Actor2d
//bei inactive evtl jegliche bewegung deaktivieren
public class ThrowableObject : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    public enum ThrowableState { Inactive, TravellingToPlayer, PickedUp, Thrown } 

    //************************//
    //    Inspector Fields    //
    //************************//

    //[SerializeField] [Range(1, 3)] private float m_speedMultiplier = 1.4f; //später per object type einstellen

    //******************//
    //    Properties    //
    //******************//

    public ThrowableState currentObjectState
    {
        get { return m_currentObjectState; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private ThrowableState m_currentObjectState = ThrowableState.Inactive;

    private Vector2 m_currentVelocity;
    private Transform m_objectToFollow;
    private float m_speed;
    private float m_targetReachedTolerance;
    private Actor2D m_actor;
    private Rigidbody2D m_rb;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {
        m_actor = GetComponent<Actor2D>();
        m_rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (currentObjectState) {
            case ThrowableState.TravellingToPlayer: {
                Vector2 objectVelocity = (m_objectToFollow.transform.position - transform.position).normalized * m_speed;
                m_rb.velocity = objectVelocity;
                if (Vector2.Distance(transform.position, m_objectToFollow.transform.position) < m_targetReachedTolerance) {
                    m_currentObjectState = ThrowableState.PickedUp;
                }
                break;
            }
            case ThrowableState.PickedUp: {
                m_rb.MovePosition(m_objectToFollow.GetComponent<Rigidbody2D>().position);
                break;
            }
            case ThrowableState.Inactive: {
                break;
            }
            case ThrowableState.Thrown: {
                CheckEnemyHit();
                GetComponent<SpriteRenderer>().color = Color.yellow;
                if (m_actor.contacts.above || m_actor.contacts.below || m_actor.contacts.left || m_actor.contacts.right) {
                    m_currentVelocity = Vector2.zero;
                    m_rb.velocity = m_currentVelocity;
                    m_currentObjectState = ThrowableState.Inactive;
                    GetComponent<SpriteRenderer>().color = Color.blue;
                }
                break;
            }
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void CheckEnemyHit()
    {
        Transform enemy = null;
        if (m_actor.contacts.below && m_actor.contacts.below.CompareTag("Enemy")) {
            enemy = m_actor.contacts.below;
        }
        if (m_actor.contacts.above && m_actor.contacts.above.CompareTag("Enemy")) {
            enemy = m_actor.contacts.above;
        }
        if (m_actor.contacts.left && m_actor.contacts.left.CompareTag("Enemy")) {
            enemy = m_actor.contacts.left;
        }
        if (m_actor.contacts.right && m_actor.contacts.right.CompareTag("Enemy")) {
            enemy = m_actor.contacts.right;
        }
        if (enemy != null) {
            enemy.GetComponent<Enemy>().GetHit(transform, 25, 4); //besser machen --> direction object zu enemy + knockback force oder so ausrechnen //4 auch als parameter hit priority übergeben
        }
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void PickUp(Transform _target, float _speed, float _targetReachedTolerance)
    {
        m_objectToFollow = _target;
        m_currentObjectState = ThrowableState.TravellingToPlayer;
        m_speed = _speed;
        m_targetReachedTolerance = _targetReachedTolerance;
    }

    public void Throw(Vector2 _velocity) // nur ein parameter 
    {
        m_rb.velocity = _velocity;
        m_currentObjectState = ThrowableState.Thrown;
    }

    public void Drop()
    {
        m_currentObjectState = ThrowableState.Inactive;
        m_objectToFollow = null;
        m_actor.velocity = Vector2.zero;
    }
}
