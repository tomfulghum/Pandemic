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

    //private MovingObjectManager manager = null;
    private Vector2 translation = Vector2.zero;
    private Rigidbody2D m_rb;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        //manager = MovingObjectManager.Instance;
        //manager.Register(this);
    }

    private void OnDestroy()
    {
        //manager.Deregister(this);
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    collision.transform.parent = transform;
    //}
    //
    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    collision.transform.parent = null;
    //}

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
