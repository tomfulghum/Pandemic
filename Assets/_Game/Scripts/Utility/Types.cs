using UnityEngine;

// Struct to store collision information for a box collider
[System.Serializable]
public struct CollisionInfo
{
    public bool above;
    public bool below;
    public bool left;
    public bool right;
}

// Enum to identify direction of collision
public enum CollisionDirection
{
    ABOVE,
    BELOW,
    LEFT,
    RIGHT
}

// Struct to store information about a single collision
[System.Serializable]
public struct CollisionData
{
    public CollisionDirection direction;
    public Transform transform;
}