using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]

public class MovingObject : MonoBehaviour
{
    //******************//
    //    Properties    //
    //******************//

    public Vector2 velocity { get; set; }
    public Vector2 deltaPosition { get; private set; }

    private Vector2 translation;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Update()
    {
        deltaPosition = velocity * Time.deltaTime + translation;

        transform.Translate(deltaPosition);
        Physics2D.SyncTransforms();

        translation = Vector2.zero;
    }

    public void Translate(Vector2 _translation)
    {
        translation = _translation;
    }
}
