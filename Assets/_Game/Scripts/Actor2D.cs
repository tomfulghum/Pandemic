using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

[RequireComponent(typeof(BoxCollider2D))]

public class Actor2D : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    [System.Serializable]
    private struct CollisionMasks
    {
        public LayerMask moving;
        public LayerMask above;
        public LayerMask below;
        public LayerMask left;
        public LayerMask right;
    }
    
    [System.Serializable]
    private struct RayBounds
    {
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
        public Vector2 topLeft;
        public Vector2 topRight;
    }

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private CollisionMasks collisionMasks = new CollisionMasks{ moving = default, above = default, below = default, left = default, right = default };
    [SerializeField] private float rayInset = 0.015f;
    [SerializeField] private int horizontalRayCount = 4;
    [SerializeField] private int verticalRayCount = 4;
    [SerializeField] private float deltaPositionThreshold = 0.001f;

    //******************//
    //    Properties    //
    //******************//

    public Vector2 velocity
    {
        get { return m_velocity; }
        set { m_velocity = value; }
    }

    public ContactData contacts
    {
        get { return m_contacts; }
    }

    public MovingObject master
    {
        private get { return m_master; }
        set { m_master = value; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private BoxCollider2D m_coll;
    private Rigidbody2D m_rb;
    private RayBounds rayBounds;

    private float horizontalRaySpacing = 0f;
    private float verticalRaySpacing = 0f;

    // Backing fields
    private Vector2 m_velocity;
    private ContactData m_contacts;
    private MovingObject m_master;

    private List<Collider2D> m_contactBuffer = new List<Collider2D>();

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_coll = GetComponent<BoxCollider2D>();
        m_rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        UpdateContacts();
    }

    private void Update()
    {

        /*
        ResetCollisions();

        Vector2 deltaPosition = m_velocity * Time.deltaTime;
        if (master) {
            Vector2 masterDelta = master.deltaPosition;
            if (masterDelta.y > 0) {
                transform.Translate(new Vector2(0, masterDelta.y));
                Physics2D.SyncTransforms();
                masterDelta.y = 0;
            }
            deltaPosition += masterDelta;
        }

        CorrectVerticalOverlap();
        UpdateRayOrigins();

        CalculateHorizontalCollisions(ref deltaPosition);
        CalculateVerticalCollisions(ref deltaPosition);

        transform.Translate(deltaPosition);
        Physics2D.SyncTransforms();
        */
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.transform);
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void UpdateContacts()
    {
        ResetCollisions();
        var bounds = m_coll.bounds;

        int contactCount = m_rb.GetContacts(m_contactBuffer);
        for (int i = 0; i < contactCount; i++) {
            var colliderDistance = m_coll.Distance(m_contactBuffer[i]);
            Vector2 contactPoint = colliderDistance.pointA;
            if (contactPoint.x <= bounds.min.x) {
                m_contacts.left = m_contactBuffer[i].transform;
            } else if (contactPoint.x >= bounds.max.x) {
                m_contacts.right = m_contactBuffer[i].transform;
            } else if (contactPoint.y <= bounds.min.y) {
                m_contacts.below = m_contactBuffer[i].transform;
            } else if (contactPoint.y >= bounds.max.y) {
                m_contacts.above = m_contactBuffer[i].transform;
            }
        }

        if (m_contacts.left || m_contacts.right || m_contacts.below || m_contacts.above) {
            m_contacts.any = true;
        }
    }

    // Updates the raycast bounds and the ray spacing
    private void UpdateRayOrigins()
    {
        Bounds collBounds = m_coll.bounds;
        collBounds.Expand(rayInset * -2f);

        rayBounds.bottomLeft = new Vector2(collBounds.min.x, collBounds.min.y);
        rayBounds.bottomRight = new Vector2(collBounds.max.x, collBounds.min.y);
        rayBounds.topLeft = new Vector2(collBounds.min.x, collBounds.max.y);
        rayBounds.topRight = new Vector2(collBounds.max.x, collBounds.max.y);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = collBounds.size.x / (horizontalRayCount - 1);
        verticalRaySpacing = collBounds.size.y / (verticalRayCount - 1);
    }

    // Modifies the deltaPosition based on horizontal collisions
    private void CalculateHorizontalCollisions(ref Vector2 _deltaPosition)
    {
        if (_deltaPosition.x == 0f) {
            return;
        }

        Vector2 rayOriginCorner = _deltaPosition.x < 0 ? rayBounds.bottomLeft : rayBounds.bottomRight;
        float rayDirection = Mathf.Sign(_deltaPosition.x);
        float rayLength = Mathf.Abs(_deltaPosition.x) + rayInset;
        LayerMask mask = rayDirection == 1 ? collisionMasks.right : collisionMasks.left;

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = rayOriginCorner + Vector2.up * (i * verticalRaySpacing);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * rayDirection, rayLength, mask);

            if (hit) {
                float deltaX = (hit.distance - rayInset) * rayDirection;
                _deltaPosition.x = Mathf.Abs(deltaX) >= deltaPositionThreshold ? deltaX : 0f;
                rayLength = hit.distance;

                if (rayDirection == 1) {
                    m_contacts.right = hit.transform;
                } else {
                    m_contacts.left = hit.transform;
                }
            }

            Debug.DrawRay(rayOrigin, Vector2.right * rayDirection * rayLength, Color.red);
        }
    }

    // Modifies the deltaPosition based on vertical collisions
    private void CalculateVerticalCollisions(ref Vector2 _deltaPosition)
    {
        if (_deltaPosition.y == 0f) {
            return;
        }

        Vector2 rayOriginCorner = _deltaPosition.y < 0 ? rayBounds.bottomLeft : rayBounds.topLeft;
        float rayDirection = Mathf.Sign(_deltaPosition.y);
        float rayLength = Mathf.Abs(_deltaPosition.y) + rayInset;
        LayerMask mask = rayDirection == 1 ? collisionMasks.above : collisionMasks.below;

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = rayOriginCorner + Vector2.right * (i * horizontalRaySpacing);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * rayDirection, rayLength, mask);

            if (hit) {
                float deltaY = (hit.distance - rayInset) * rayDirection;
                _deltaPosition.y = Mathf.Abs(deltaY) >= deltaPositionThreshold ? deltaY : 0f;
                rayLength = hit.distance;

                if (rayDirection == 1) {
                    m_contacts.above = hit.transform;
                } else {
                    m_contacts.below = hit.transform;
                }
            }

            Debug.DrawRay(rayOrigin, Vector2.up * rayDirection * rayLength, Color.red);
        }
    }

    private void CorrectVerticalOverlap()
    {
        Bounds collBounds = m_coll.bounds;
        Vector2 overlapSize = new Vector2(collBounds.max.x - collBounds.min.x, collBounds.max.y - collBounds.min.y);
        Collider2D overlap = Physics2D.OverlapBox(transform.position, overlapSize, 0, collisionMasks.moving);
        if (overlap) {
            MovingObject movingObject = overlap.GetComponent<MovingObject>();
            ColliderDistance2D colliderDistance = m_coll.Distance(overlap);
            if (movingObject.velocity.y > 0 && colliderDistance.distance < 0) {
                transform.Translate(colliderDistance.distance * colliderDistance.normal);
                Physics2D.SyncTransforms();
            }
        }
    }

    // Sets all collision variables to false
    private void ResetCollisions()
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

