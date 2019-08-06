
[System.Serializable]
public class PlayerState
{
    public Area currentArea;
    public int currentTransitionId ;
    public int score;

    public PlayerState(Area _currentArea)
    {
        currentArea = _currentArea;
        currentTransitionId = 0;
        score = 0;
    }
}
