
[System.Serializable]
public class PlayerState
{
    public string currentSpawnPoint;
    public int normalKeyCount;

    public PlayerState(SpawnPointData _currentSpawnPoint)
    {
        currentSpawnPoint = _currentSpawnPoint.id;
        normalKeyCount = 0;
    }
}
