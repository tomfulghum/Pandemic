using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAreaData", menuName = "Pandemic/Area Data")]
public class AreaData : ScriptableObject
{
    [UniqueIdentifier]
    [SerializeField] private string m_id = default;
    [SerializeField] private string m_sceneName = default;
    [SerializeField] private List<SpawnPointData> m_spawnPoints = default;

    public string id
    {
        get { return m_id; }
    }

    public string sceneName
    {
        get { return m_sceneName; }
    }

    public List<SpawnPointData> spawnPoints
    {
        get { return m_spawnPoints; }
    }

    public SpawnPointData FindSpawnPoint(string _id)
    {
        return m_spawnPoints.Find(x => x.id.Equals(_id));
    }
}
