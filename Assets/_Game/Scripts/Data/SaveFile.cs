public class SaveFileData
{
    public int health { get; private set; } = -1;
    public int currency { get; private set; } = -1;
    public float playTime { get; private set; } = -1;
    public string area { get; private set; } = "INVALID";

    public SaveFileData(GameState _state, string _area)
    {
        //health = _state.playerState.health;
        //currency = _state.playerState.currency;
        //playTime = _state.playTime;
        area = _area;
    }
}
