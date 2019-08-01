﻿using System;
using Rewired;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    
    private GameObject myPlayer;
    private Vector3 v3OldPos;
    private int iFrames;

    private void Start()
    {
        myPlayer = GameObject.Find("Player");
        v3OldPos = myPlayer.transform.position;
        iFrames = 0;
    }

    private void Update()
    {
        iFrames++;
        if (iFrames >= 90 && myPlayer.GetComponent<Actor2D>().contacts.below)
        {
                v3OldPos = myPlayer.transform.position;
                iFrames = 0;
                Debug.Log("Saved");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.CompareTag("Player"))
            other.transform.position = v3OldPos;
    }
    
}
