using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//prinzipiell dasselbe wie enemy spawner sollte später zusammengeführt werden
public class ObjectSpawner : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private bool m_spawn = true;
    [SerializeField] private int m_maxNumOfActiveItems = 5;
    [SerializeField] private GameObject m_objectToSpawn;

    //**********************//
    //    Private Fields    //
    //**********************//

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
