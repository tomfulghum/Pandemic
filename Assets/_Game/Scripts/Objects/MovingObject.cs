using UnityEngine;

public class MovingObject : MonoBehaviour
{
    //******************//
    //    Properties    //
    //******************//

    public Vector2 velocity { get; set; }
    public Vector2 deltaPosition { get; private set; }

    //**********************//
    //    Private Fields    //
    //**********************//

    private MovingObjectManager manager = null;
    private Vector2 translation = Vector2.zero;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        manager = MovingObjectManager.Instance;
        manager.Register(this);
    }

    private void OnDestroy()
    {
        manager.Deregister(this);
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void UpdateTransform()
    {
        deltaPosition = velocity * Time.deltaTime + translation;
        transform.Translate(deltaPosition);

        translation = Vector2.zero;
    }

    public void Translate(Vector2 _translation)
    {
        translation = _translation;
    }
}
