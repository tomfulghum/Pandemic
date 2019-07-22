﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingCredits : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] int scrollSpeedPerSecond = 100;
    [SerializeField] int delay = 1;

    //**********************//
    //    Private Fields    //
    //**********************//

    private RectTransform credits;
    private Vector3 maxVector;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        credits = GetComponent<RectTransform>();
    }

    private void Start()
    {
        maxVector = new Vector3(0, delay, 0);
    }

    void OnEnable()
    {
        credits.anchoredPosition3D = new Vector3(0, -2900, 0);
    }

    void Update()
    {
        credits.anchoredPosition3D += new Vector3(0, scrollSpeedPerSecond * Time.deltaTime, 0);

        if (credits.anchoredPosition3D.y > maxVector.y) {
            credits.anchoredPosition3D = new Vector3(0, -2530, 0);
        }
    }
}