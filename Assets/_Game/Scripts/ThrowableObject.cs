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
    [SerializeField] private float m_rotationOffset = 0;
    [SerializeField] private bool m_destructionColorGreen = false; 
    [SerializeField] private GameObject m_destructionEffect = default; //vllt noch nicht sogut gelöst aber fürs erste reichts

    //******************//
    //    Properties    //
    //******************//

    public ThrowableState currentObjectState
    {
        get { return m_currentObjectState; }
    }

    public float rotationOffset
    {
        get { return m_rotationOffset; }
        set { m_rotationOffset = value; }
    }

    public bool pickable
    {
        get { return m_pickable; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private ThrowableState m_currentObjectState = ThrowableState.Inactive;
    private bool m_pickable = true;

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

    //void Start()
    //{
    //    m_actor = GetComponent<Actor2D>();
    //    m_rb = GetComponent<Rigidbody2D>();
    //    m_okb = GetComponentInChildren<ObjectKnockBack>();
    //}

    private void Awake() //could be better for instantiate
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
                    CorrectRotation(180);
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
                    if (m_actor.contacts.any)
                    {
                        m_rb.velocity = Vector2.zero; //könnte das object dadurch an der decke kleben?
                        if(m_hitGround == false) //facotored den drop nicht mit ein / schlimm?
                        {
                            SubstractDurability();
                            m_hitGround = true;
                        }
                    }
                    break;
                }
            case ThrowableState.Thrown:
                {
                    CorrectRotation();
                    //GetComponent<SpriteRenderer>().color = Color.yellow;
                    if (m_actor.contacts.any)
                    {
                        SetInactive(); //abbprallen bei den leben mit einberechnen
                    }
                    break;
                }
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void CorrectRotation(float _additionalRotation = 0)
    {
        Vector2 moveDirection = m_rb.velocity;
        if (moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle + m_rotationOffset + _additionalRotation, Vector3.forward);
        }
    }

    private void ResetRotation()
    {
        transform.rotation = Quaternion.identity;
        GetComponent<SpriteRenderer>().flipY = false; 
    }

    private void AdjustSprite(Vector2 _velocity)
    {
        if (_velocity.x < 0)
        {
            GetComponent<SpriteRenderer>().flipY = true;
            m_rotationOffset = -Mathf.Abs(m_rotationOffset);
        } else
        {
            GetComponent<SpriteRenderer>().flipY = false;
            m_rotationOffset = Mathf.Abs(m_rotationOffset);
        }
    }

    private void SubstractDurability()
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
    }

    private void SetInactive()
    {
        m_okb.IsLethal(false);
        m_rb.velocity = Vector2.zero;
        m_rb.gravityScale = 1f;
        m_currentObjectState = ThrowableState.Inactive;
        //GetComponent<SpriteRenderer>().color = Color.blue;
        m_pickable = true;
        ResetRotation();
        SubstractDurability();
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

    public void Throw(Vector2 _velocity, bool _isLethal, float _customSpeedMultiplier = -1, bool _pickableInAir = false) // nur ein parameter 
    {
        if (_customSpeedMultiplier < 0)
            _customSpeedMultiplier = m_speedMultiplier;
        m_rb.velocity = _velocity * _customSpeedMultiplier;
        m_rb.gravityScale = 1 * Mathf.Pow(_customSpeedMultiplier, 2);
        m_currentObjectState = ThrowableState.Thrown;
        m_rb.isKinematic = false;
        m_hitGround = false;
        m_pickable = _pickableInAir;
        m_okb.IsLethal(_isLethal);
        AdjustSprite(_velocity);
    }

    public void Drop()
    {
        m_currentObjectState = ThrowableState.Inactive;
        m_rb.isKinematic = false;
        m_objectToFollow = null;
        m_pickable = true;
        m_rb.velocity = Vector2.zero;
    }
}
