using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public PlayerState playerState;
    public List<AreaState> areaStates;

    public GameState(Area _currentArea)
    {
        playerState = new PlayerState(_currentArea);
        areaStates = new List<AreaState>();
    }

    public AreaState GetAreaState(Area _area)
    {
        return areaStates.Find(x => x.area == _area.id);
    }
}
