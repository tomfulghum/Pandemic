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

    public CollisionData collision
    {
        get { return m_collisions; }
    }

    public MovingObject master
    {
        private get { return m_master; }
        set { m_master = value; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    [HideInInspector]
    private BoxCollider2D coll;
    private RayBounds rayBounds;

    private float horizontalRaySpacing = 0f;
    private float verticalRaySpacing = 0f;

    // Backing fields
    private Vector2 m_velocity;
    private CollisionData m_collisions;
    private MovingObject m_master;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
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

        Bounds collBounds = coll.bounds;
        Vector2 overlapSize = new Vector2(collBounds.max.x - collBounds.min.x, collBounds.max.y - collBounds.min.y);
        Collider2D overlap = Physics2D.OverlapBox(transform.position, overlapSize, 0, collisionMasks.moving);
        if (overlap) {
            MovingObject movingObject = overlap.GetComponent<MovingObject>();
            ColliderDistance2D colliderDistance = coll.Distance(overlap);
            if (movingObject.velocity.y > 0 && colliderDistance.distance < 0) {
                transform.Translate(colliderDistance.distance * colliderDistance.normal);
                Physics2D.SyncTransforms();
            }
        }

        UpdateRayOrigins();

        CalculateHorizontalCollisions(ref deltaPosition);
        CalculateVerticalCollisions(ref deltaPosition);

        transform.Translate(deltaPosition);
        Physics2D.SyncTransforms();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.transform);
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    // Updates the raycast bounds and the ray spacing
    private void UpdateRayOrigins()
    {
        Bounds collBounds = coll.bounds;
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
                    m_collisions.right = hit.transform;
                } else {
                    m_collisions.left = hit.transform;
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
                    m_collisions.above = hit.transform;
                } else {
                    m_collisions.below = hit.transform;
                }
            }

            Debug.DrawRay(rayOrigin, Vector2.up * rayDirection * rayLength, Color.red);
        }
    }

    // Sets all collision variables to false
    private void ResetCollisions()
    {
        m_collisions.above = null;
        m_collisions.below = null;
        m_collisions.right = null;
        m_collisions.left = null;
    }

    //************************//
    //    Public Functions    //
    //************************//
}

