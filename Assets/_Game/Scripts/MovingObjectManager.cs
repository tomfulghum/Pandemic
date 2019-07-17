using System.Collections.Generic;
using UnityEngine;

public class MovingObjectManager : MonoBehaviour
{
    //******************//
    //    Properties    //
    //******************//

    public static MovingObjectManager Instance { get; private set; }

    //**********************//
    //    Private Fields    //
    //**********************//

    private HashSet<MovingObject> objects = new HashSet<MovingObject>();

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(this);
        }
    }

    void Update()
    {
        foreach (MovingObject obj in objects) {
            obj.UpdateTransform();
        }
        Physics2D.SyncTransforms();
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void Register(MovingObject _movingObject)
    {
        objects.Add(_movingObject);
    }

    public void Deregister(MovingObject _movingObject)
    {
        objects.Remove(_movingObject);
    }
}
