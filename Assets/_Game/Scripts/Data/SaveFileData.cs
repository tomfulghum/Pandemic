
[System.Serializable]
public class SaveFileData
{
    public GameState state { get; private set; }

    public int health { get; private set; } = -1;
    public int currency { get; private set; } = -1;
    public float playTime { get; private set; } = -1;
    public string area { get; private set; } = "INVALID";

    public SaveFileData(GameState _state, string _area)
    {
        state = _state;
        health = _state.playerState.health;
        currency = _state.playerState.normalKeyCount;
        //playTime = _state.playTime;
        area = _area;
    }
}
