using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFloating : MonoBehaviour
{
    public float speed;
    public float height;

    Vector2 startPosition;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = startPosition + new Vector2(0 , Mathf.Sin(Time.fixedTime  * speed) * height);
    }
}