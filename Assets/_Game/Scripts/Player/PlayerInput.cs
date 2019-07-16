using UnityEngine;
using Rewired;

public class PlayerInput : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private int playerId = 0;
    [SerializeField] private InputBindings bindings = default;

    //******************//
    //    Properties    //
    //******************//

    public Player player { get; private set; }
    public string jumpButton { get { return bindings.jumpButtonName; } }
    public string hookButton { get { return bindings.hookButtonName; } }
    public string moveHorizontalAxis { get { return bindings.moveHorizontalAxisName; } }
    public string aimHorizontalAxis { get { return bindings.aimHorizontalAxisName; } }
    public string aimVerticalAxis { get { return bindings.aimVerticalAxisName; } }

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        player = ReInput.players.GetPlayer(playerId);
    }
}
