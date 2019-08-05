using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private int m_healthBack = 2;


    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] colliderInRange = Physics2D.OverlapBoxAll(transform.position, GetComponent<BoxCollider2D>().size * transform.localScale, 0);
        foreach(Collider2D col in colliderInRange)
        {
            if(col.CompareTag("Player"))
            {
                col.gameObject.GetComponent<PlayerCombat>().HealUp(m_healthBack);
                Destroy(gameObject);
            }
        }
    }
}
