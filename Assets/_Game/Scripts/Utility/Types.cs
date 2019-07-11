using UnityEngine;

// Struct to store collision information for a box collider
[System.Serializable]
public struct CollisionData
{
    public Transform above;
    public Transform below;
    public Transform left;
    public Transform right;
}

// Enum to identify direction of collision
public enum CollisionDirection
{
    ABOVE,
    BELOW,
    LEFT,
    RIGHT
}