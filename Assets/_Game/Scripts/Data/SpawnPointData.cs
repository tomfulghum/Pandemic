using UnityEngine;

[CreateAssetMenu(fileName = "NewSpawnPointData", menuName = "Pandemic/Spawn Point Data")]
public class SpawnPointData : ScriptableObject
{
    [UniqueIdentifier]
    [SerializeField] private string m_id = default;
    [SerializeField] private AreaData m_area = default;

    public string id
    {
        get { return m_id; }
    }

    public AreaData area
    {
        get { return m_area; }
    }
}
