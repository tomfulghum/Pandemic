using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingCredits : MonoBehaviour
{
    [SerializeField] int ScrollSpeedPerSecond;
    [SerializeField] int Delay;
    RectTransform Credits;
    Vector3 maxVector;

    private void Awake()
    {
        Credits = GetComponent<RectTransform>();
    }

    private void Start()
    {
        maxVector = new Vector3(0, Delay, 0);
    }

    void OnEnable()
    {
        Credits.anchoredPosition3D = new Vector3(0, -2900, 0);
    }

    void Update()
    {
        Credits.anchoredPosition3D += new Vector3(0, ScrollSpeedPerSecond * Time.deltaTime, 0);

        if (Credits.anchoredPosition3D.y > maxVector.y)
        {
            Credits.anchoredPosition3D = new Vector3(0, -2530, 0);
        }
    }
}
