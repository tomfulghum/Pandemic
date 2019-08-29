using UnityEngine;

// Struct to store collision information for a box collider
[System.Serializable]
public struct ContactData
{
    public Transform above;
    public Transform below;
    public Transform left;
    public Transform right;
    public bool any;
}

// Enum to identify direction of collision
public enum CollisionDirection
{
    ABOVE,
    BELOW,
    LEFT,
    RIGHT
}

// Used to assign unique identifiers to scriptable objects
public class UniqueIdentifierAttribute : PropertyAttribute { }