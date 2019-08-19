using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//requires Component Actor2d
//bei inactive evtl jegliche bewegung deaktivieren
//object to follow wird auch nach dem throw nicht auf null gesetzt schlimm?
public class ThrowableObject : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    public enum ThrowableState { Inactive, TravellingToPlayer, PickedUp, Thrown }

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] [Range(1, 2)] private float m_speedMultiplier = 1.3f; //später per object type einstellen
    [SerializeField] private int m_throwCount = 1;
    [SerializeField] private bool m_destructionColorGreen = false; 
    [SerializeField] private GameObject m_destructionEffect = default; //vllt noch nicht sogut gelöst aber fürs erste reichts

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

    private Transform m_objectToFollow;
    private float m_speed;
    private bool m_hitGround = false;
    private float m_targetReachedTolerance;

    private Actor2D m_actor;
    private Rigidbody2D m_rb;
    private ObjectKnockBack m_okb;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {
        m_actor = GetComponent<Actor2D>();
        m_rb = GetComponent<Rigidbody2D>();
        m_okb = GetComponentInChildren<ObjectKnockBack>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (currentObjectState)
        {
            case ThrowableState.TravellingToPlayer:
                {
                    Vector2 objectVelocity = (m_objectToFollow.transform.position - transform.position).normalized * m_speed;
                    m_rb.velocity = objectVelocity;
                    if (Vector2.Distance(transform.position, m_objectToFollow.transform.position) < m_targetReachedTolerance)
                    {
                        m_currentObjectState = ThrowableState.PickedUp;
                    }
                    break;
                }
            case ThrowableState.PickedUp:
                {
                    m_rb.MovePosition(m_objectToFollow.GetComponent<Rigidbody2D>().position);
                    break;
                }
            case ThrowableState.Inactive:
                {
                    if (m_actor.contacts.above || m_actor.contacts.below || m_actor.contacts.left || m_actor.contacts.right)
                    {
                        m_rb.velocity = Vector2.zero; //könnte das object dadurch an der decke kleben?
                        if(m_hitGround == false) //facotored den drop nicht mit ein / schlimm?
                        {
                            m_throwCount--;
                            if (m_throwCount <= 0)
                            {
                                GameObject destructionEffect = Instantiate(m_destructionEffect, transform.position, transform.rotation);
                                destructionEffect.transform.localScale = transform.localScale;
                                if (m_destructionColorGreen)
                                    destructionEffect.GetComponent<Animator>().SetFloat("ColorGreen", 1f);
                                Destroy(destructionEffect, destructionEffect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
                                Destroy(gameObject);
                            }
                            m_hitGround = true;
                        }
                    }
                    break;
                }
            case ThrowableState.Thrown:
                {
                    GetComponent<SpriteRenderer>().color = Color.yellow;
                    if (m_actor.contacts.above || m_actor.contacts.below || m_actor.contacts.left || m_actor.contacts.right)
                    {
                        SetInactive();
                    }
                    break;
                }
        }
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void PickUp(Transform _target, float _speed, float _targetReachedTolerance)
    {
        m_rb.velocity = Vector2.zero;
        m_objectToFollow = _target;
        m_currentObjectState = ThrowableState.TravellingToPlayer;
        m_speed = _speed;
        m_targetReachedTolerance = _targetReachedTolerance;
        m_rb.isKinematic = true;
    }

    public void Throw(Vector2 _velocity) // nur ein parameter 
    {
        m_rb.velocity = _velocity * m_speedMultiplier;
        m_rb.gravityScale *= Mathf.Pow(m_speedMultiplier, 2);
        m_currentObjectState = ThrowableState.Thrown;
        m_rb.isKinematic = false;
        m_hitGround = false;
        m_okb.IsLethal(true);
    }

    public void Drop()
    {
        m_currentObjectState = ThrowableState.Inactive;
        m_rb.isKinematic = false;
        m_objectToFollow = null;
        m_rb.velocity = Vector2.zero;
    }

    public void SetInactive()
    {
        m_okb.IsLethal(false);
        m_rb.velocity = Vector2.zero;
        m_rb.gravityScale = 1f;
        m_currentObjectState = ThrowableState.Inactive;
        GetComponent<SpriteRenderer>().color = Color.blue;
    }
}
