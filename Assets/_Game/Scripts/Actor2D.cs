using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]

public class Actor2D : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    [System.Serializable]
    private struct ContactMasks
    {
        public LayerMask left;
        public LayerMask right;
        public LayerMask below;
        public LayerMask above;
    }

    // whooop

    //************************//
    //    Inspector Fields    //
    //************************//

    [Tooltip("Filter to determine in which layers Rigidbody2D contacts are actually counted as contact.")]
    [SerializeField] private ContactMasks m_contactFilter = new ContactMasks { left = default, right = default, below = default, above = default };

    //******************//
    //    Properties    //
    //******************//

    public ContactData contacts
    {
        get { return m_contacts; }
    }

    public Rigidbody2D master
    {
        get; set;
    }

    public Vector2 velocity // Legacy to avoid compiler errors, will be removed
    {
        get; set;
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private BoxCollider2D m_coll;
    private Rigidbody2D m_rb;

    private List<Collider2D> m_contactBuffer = new List<Collider2D>();

    // Backing fields
    private ContactData m_contacts;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_coll = GetComponent<BoxCollider2D>();
        m_rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        UpdateContacts();

        if (m_contacts.below && m_contacts.below.CompareTag("MovingObject")) {
            master = m_contacts.below.GetComponent<Rigidbody2D>();
        } else if (master) {
            master = null;
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    // Checks all Rigidbody2D contacts for validity and sets contacts accordingly.
    private void UpdateContacts()
    {
        ResetContacts();
        var bounds = m_coll.bounds;

        int contactCount = m_rb.GetContacts(m_contactBuffer);
        for (int i = 0; i < contactCount; i++) {
            var colliderDistance = m_coll.Distance(m_contactBuffer[i]);
            Vector2 contactPoint = colliderDistance.pointA;
            int layer = m_contactBuffer[i].gameObject.layer;

            if (Mathf.Abs(colliderDistance.distance) < Physics2D.defaultContactOffset) {
                if (contactPoint.x <= bounds.min.x && m_contactFilter.left.Contains(layer)) {
                    m_contacts.left = m_contactBuffer[i].transform;
                } else if (contactPoint.x >= bounds.max.x && m_contactFilter.right.Contains(layer)) {
                    m_contacts.right = m_contactBuffer[i].transform;
                } else if (contactPoint.y <= bounds.min.y && m_contactFilter.below.Contains(layer)) {
                    m_contacts.below = m_contactBuffer[i].transform;
                } else if (contactPoint.y >= bounds.max.y && m_contactFilter.above.Contains(layer)) {
                    m_contacts.above = m_contactBuffer[i].transform;
                }
            }
        }

        if (m_contacts.left || m_contacts.right || m_contacts.below || m_contacts.above) {
            m_contacts.any = true;
        }
    }

    // Sets all collision variables to false.
    private void ResetContacts()
    {
        m_contacts.left = null;
        m_contacts.right = null;
        m_contacts.below = null;
        m_contacts.above = null;
        m_contacts.any = false;
    }

    //************************//
    //    Public Functions    //
    //************************//
}

