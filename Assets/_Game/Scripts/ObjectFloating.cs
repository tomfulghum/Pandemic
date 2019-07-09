using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFloating : MonoBehaviour
{
    public enum Pattern { Vertical, Horizontal, Circular }
    public Pattern MovementPattern;
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
        switch (MovementPattern)
        {
            case Pattern.Vertical:
                {
                    transform.position = startPosition + new Vector2(0, Mathf.Sin(Time.fixedTime * speed) * height);
                    break;
                }
            case Pattern.Horizontal:
                {
                    transform.position = startPosition + new Vector2(Mathf.Sin(Time.fixedTime * speed) * height, 0);
                    break;
                }
            case Pattern.Circular: //center is start position
                {
                    transform.position = startPosition + new Vector2(Mathf.Sin(Time.fixedTime * speed) * height, Mathf.Cos(Time.fixedTime * speed) * height);
                    break;
                }
        }
    }
}