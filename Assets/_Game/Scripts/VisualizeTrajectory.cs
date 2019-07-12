﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeTrajectory : MonoBehaviour
{
    [Range(0,100)] public int NumOfVisualDots;
    public GameObject DotPrefab;
    GameObject DotParent;
    // Start is called before the first frame update
    void Start()
    {
        DotParent = new GameObject("Parent Dot"); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void VisualizeDots(Vector2 _startPosition, Vector2 _launchVelocity ,float _gravity) //gravity sollte irgendwoanders gesetzt werden --> am besten von dem object das geworfen wird
    {
        RemoveVisualeDots();
        //float TravelTime = CalculateTravelTime( _startPosition,  _launchVelocity, new Vector2(0, -_gravity));
        //float TravelTimeForEachDot = TravelTime / NumOfVisualDots;
        DotParent.transform.position = _startPosition;
        for (int i = 0; i < NumOfVisualDots; i++)
        {
            GameObject trajectoryDot = Instantiate(DotPrefab);
            trajectoryDot.transform.SetParent(DotParent.transform);
            trajectoryDot.transform.position = CalculatePosition(0.05f * i, _launchVelocity, DotParent.transform.position, new Vector2(0, -_gravity));

        }
    }

    public void RemoveVisualeDots()
    {
        foreach (Transform child in DotParent.transform) //vllt später jedesmal wieder die gleichen objects benutzen -->object pooling --> check if num of visual dots == num of childs ansonsten neue erstellen usw.
        {
            Destroy(child.gameObject);
        }
    }

    Vector2 CalculatePosition(float elapsedTime, Vector2 _launchVelocity, Vector2 _initialPosition, Vector2 _gravity)
    {
        return _gravity * elapsedTime * elapsedTime * 0.5f + _launchVelocity * elapsedTime + _initialPosition;
    }

    float CalculateTravelTime(Vector2 _startPosition, Vector2 _launchVelocity, Vector2 _gravity) // a = gravity/2, b = start velocity, c = start position //not really working 
    {
        float x = Mathf.Pow(_launchVelocity.y, 2) - 4 * -_gravity.y * 0.5f * _startPosition.y;
        float time = (-_launchVelocity.y - Mathf.Sqrt(x)) / 2 * _gravity.y * 0.5f;
        return time;
    }
}
