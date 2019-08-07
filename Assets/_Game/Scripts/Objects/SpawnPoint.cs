using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private SpawnPointData m_spawnPointData = default;

    public SpawnPointData spawnPointData
    {
        get { return m_spawnPointData; }
    }

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
