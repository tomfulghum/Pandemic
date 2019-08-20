using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameBrawler_Flames : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float m_lifeTime = 10f;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//
    void Start()
    {
        Destroy(gameObject, m_lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
