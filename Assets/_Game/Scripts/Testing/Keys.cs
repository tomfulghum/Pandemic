using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Keys : MonoBehaviour
{
    [SerializeField]
    private int m_iNormalKeys;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NormalKey") || other.CompareTag("BossKey"))
        {
            if (other.CompareTag("NormalKey"))
                m_iNormalKeys++;
            
            Destroy(other.gameObject);
        }

        if (other.CompareTag("KeyDoor") && m_iNormalKeys >= 4)
        {
            m_iNormalKeys = -4;
            Destroy(other.gameObject);
        }
    }
}
