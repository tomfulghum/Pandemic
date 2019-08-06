using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public List<AreaState> areaStates;

    public GameState()
    {
        areaStates = new List<AreaState>();
    }

    public AreaState GetAreaState(Area _area)
    {
        return areaStates.Find(x => x.area == _area);
    }
}
