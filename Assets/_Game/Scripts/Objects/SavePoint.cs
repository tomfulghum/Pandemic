using UnityEngine;

[RequireComponent(typeof(Interactable))]

public class SavePoint : MonoBehaviour
{
    [SerializeField] private SpawnPointData m_spawnPoint = default;

    public void SaveGame()
    {
        GameManager.Instance.currentSpawnPoint = m_spawnPoint;
        GameManager.Instance.SaveGame();
    }
}
