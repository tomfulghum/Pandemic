using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinusMotion : MonoBehaviour
{
    [SerializeField] float amplitude = 1f;
    [SerializeField] float frequency = 1f;

    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = startPosition + new Vector3(amplitude * Mathf.Sin(Time.time), amplitude * Mathf.Cos(Time.time * frequency));
    }
}
