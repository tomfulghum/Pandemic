using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinusMotion : MonoBehaviour
{
    [SerializeField] float amplitude = 1f;
    [SerializeField] float frequency = 1f;

    private Vector3 startPosition;
    private Rigidbody2D m_rb;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_rb.MovePosition(startPosition + new Vector3(amplitude * Mathf.Sin(Time.time), amplitude * Mathf.Cos(Time.time * frequency)));
    }
}
