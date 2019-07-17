using UnityEngine;

public class BackgroundParallaxElement : MonoBehaviour
{
    //******************//
    //    Properties    //
    //******************//

    public Vector3 initialPosition { get; private set; }

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        initialPosition = transform.localPosition;
    }
}
