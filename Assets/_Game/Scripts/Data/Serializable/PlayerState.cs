
[System.Serializable]
public class PlayerState
{
    public string currentArea;
    public int currentTransitionId;
    public int normalKeyCount;

    public PlayerState(Area _currentArea)
    {
        currentArea = _currentArea.id;
        currentTransitionId = 0;
        normalKeyCount = 0;
    }
}
