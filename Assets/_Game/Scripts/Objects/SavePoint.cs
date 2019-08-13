using UnityEngine;

[RequireComponent(typeof(Interactable))]

public class SavePoint : MonoBehaviour
{
    [SerializeField] private SpawnPoint m_spawnPoint = default;

    public void SaveGame()
    {
        GameManager.Instance.SaveGame(m_spawnPoint.spawnPointData);
    }
}
