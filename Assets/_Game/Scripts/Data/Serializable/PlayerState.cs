
[System.Serializable]
public class PlayerState
{
    public string currentSpawnPoint;
    public int normalKeyCount;
    public int health;

    public PlayerState(SpawnPointData _currentSpawnPoint)
    {
        currentSpawnPoint = _currentSpawnPoint.id;
        normalKeyCount = 0;
        health = 0;
    }
}
