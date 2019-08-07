using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public PlayerState playerState;
    public List<AreaState> areaStates;

    public GameState(SpawnPointData _currentSpawnPoint)
    {
        playerState = new PlayerState(_currentSpawnPoint);
        areaStates = new List<AreaState>();
    }

    public AreaState GetAreaState(AreaData _area)
    {
        return areaStates.Find(x => x.area == _area.id);
    }
}
