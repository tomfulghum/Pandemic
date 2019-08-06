using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public List<AreaState> areaStates;
    public PlayerState playerState;

    public GameState(Area _currentArea)
    {
        areaStates = new List<AreaState>();
        playerState = new PlayerState(_currentArea);
    }

    public AreaState GetAreaState(Area _area)
    {
        return areaStates.Find(x => x.area == _area);
    }
}
