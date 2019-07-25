using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setSpeed : MonoBehaviour
{
    public Transform tShip;
    public Transform tWaypoint2;

    private void Update()
    {
        if (tShip.position.y == tWaypoint2.position.y)
            //tShip.GetComponent<MovingPlatform>().Speed = 0f;
            tShip.GetComponent<MovingPlatform>().running = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //tShip.GetComponent<MovingPlatform>().Speed = 3f;
        tShip.GetComponent<MovingPlatform>().running = true;
    }
}
