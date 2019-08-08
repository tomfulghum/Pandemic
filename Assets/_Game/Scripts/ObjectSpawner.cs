using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//prinzipiell dasselbe wie enemy spawner sollte später zusammengeführt werden
//später besser machen --> check in der coroutine ist schlecht
public class ObjectSpawner : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private bool m_spawn = true;
    [SerializeField] private int m_maxNumOfActiveItems = 5;
    [SerializeField] private GameObject m_objectToSpawn = default;

    //**********************//
    //    Private Fields    //
    //**********************//

    private List<GameObject> m_activeObjects = new List<GameObject>();

    // Start is called before the first frame update
    private void Start()
    {
        if (m_spawn && m_objectToSpawn != null)
            StartCoroutine(SpawnEnemy());
    }

    // Update is called once per frame
    private void Update()
    {
        for(int i = m_activeObjects.Count - 1; i >= 0; i--)
        {
            if (m_activeObjects[i] == null)
                m_activeObjects.Remove(m_activeObjects[i]);
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private IEnumerator SpawnEnemy()
    {
        while (m_spawn)
        {
            yield return new WaitForSeconds(5 + Random.Range(0f, 4f));
            if (m_activeObjects.Count < m_maxNumOfActiveItems)
            {
                GameObject obj = Instantiate(m_objectToSpawn, transform.position, transform.rotation);
                m_activeObjects.Add(obj);
            }
        }
    }
}
