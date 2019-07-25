using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool Spawn;
    public GameObject enemy; //vllt als liste? ein random enemy aus der liste? 
    // Start is called before the first frame update
    void Start()
    {
        if (Spawn && enemy != null)
            StartCoroutine(SpawnEnemy());
    }

    // Update is called once per frame
    void Update()
    {
        //if (!Spawn)
            //CancelInvoke("SpawnEnemy");
    }

    IEnumerator SpawnEnemy()
    {
        while (Spawn)
        {
            yield return new WaitForSeconds(5 + Random.Range(0f, 4f));
            Instantiate(enemy, transform);
        }
    }
}
