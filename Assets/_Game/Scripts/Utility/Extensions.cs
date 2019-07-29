using UnityEngine;

public static class Extensions
{
    // Checks if a layer is included in a layer mask
    public static bool Contains(this LayerMask _mask, int _layer)
    {
        return _mask == (_mask | (1 << _layer));
    }
}
