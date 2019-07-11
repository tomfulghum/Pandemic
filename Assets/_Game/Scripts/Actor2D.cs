﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

[RequireComponent(typeof(BoxCollider2D))]

public class Actor2D : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    [System.Serializable]
    struct RayBounds
    {
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
        public Vector2 topLeft;
        public Vector2 topRight;
    }

    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private LayerMask collisionMask = default;
    [SerializeField] private float rayInset = 0.015f;
    [SerializeField] private int horizontalRayCount = 4;
    [SerializeField] private int verticalRayCount = 4;
    [SerializeField] private float deltaPositionThreshold = 0.001f;

    //****************//
    //   Properties   //
    //****************//

    public Vector2 velocity
    {
        get { return m_velocity; }
        set { m_velocity = value; }
    }

    public CollisionInfo collision
    {
        get { return m_collisions; }
    }

    public ReadOnlyCollection<CollisionData> collisionData
    {
        get { return m_roCollisionData; }
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
    private CollisionInfo m_collisions;
    private List<CollisionData> m_collisionData = new List<CollisionData>();
    private ReadOnlyCollection<CollisionData> m_roCollisionData = null;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        UpdateRayOrigins();
        ResetCollisions();

        Vector2 deltaPosition = m_velocity * Time.deltaTime;
        CalculateHorizontalCollisions(ref deltaPosition);
        CalculateVerticalCollisions(ref deltaPosition);

        m_roCollisionData = new ReadOnlyCollection<CollisionData>(m_collisionData);

        transform.Translate(deltaPosition);
        Physics2D.SyncTransforms();
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

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = rayOriginCorner + Vector2.up * (i * verticalRaySpacing);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * rayDirection, rayLength, collisionMask);

            if (hit) {
                float deltaX = (hit.distance - rayInset) * rayDirection;
                _deltaPosition.x = Mathf.Abs(deltaX) >= deltaPositionThreshold ? deltaX : 0f;
                rayLength = hit.distance;

                CollisionDirection dir = rayDirection == 1 ? CollisionDirection.RIGHT : CollisionDirection.LEFT;
                CollisionData data = new CollisionData() { direction = dir, transform = hit.transform };
                m_collisionData.Add(data);

                m_collisions.right = rayDirection == 1;
                m_collisions.left = rayDirection == -1;
            }

            Debug.DrawRay(rayOrigin + _deltaPosition, Vector2.right * rayDirection * rayLength, Color.red);
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

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = rayOriginCorner + Vector2.right * (i * horizontalRaySpacing);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * rayDirection, rayLength, collisionMask);

            if (hit) {
                float deltaY = (hit.distance - rayInset) * rayDirection;
                _deltaPosition.y = Mathf.Abs(deltaY) >= deltaPositionThreshold ? deltaY : 0f;
                rayLength = hit.distance;

                CollisionDirection dir = rayDirection == 1 ? CollisionDirection.ABOVE : CollisionDirection.BELOW;
                CollisionData data = new CollisionData() { direction = dir, transform = hit.transform };
                m_collisionData.Add(data);

                m_collisions.above = rayDirection == 1;
                m_collisions.below = rayDirection == -1;
            }

            Debug.DrawRay(rayOrigin + _deltaPosition, Vector2.up * rayDirection * rayLength, Color.red);
        }
    }

    // Sets all collision variables to false
    private void ResetCollisions()
    {
        m_collisionData.Clear();
        m_collisions.above = false;
        m_collisions.below = false;
        m_collisions.right = false;
        m_collisions.left = false;
    }
}

