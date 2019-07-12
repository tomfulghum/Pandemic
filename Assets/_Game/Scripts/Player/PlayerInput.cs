using UnityEngine;
using Rewired;

public class PlayerInput : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private int playerId = 0;
    [SerializeField] private string jumpButtonName = "Jump";
    [SerializeField] private string hookButtonName = "Hook";
    [SerializeField] private string moveHorizontalAxisName = "MoveHorizontal";
    [SerializeField] private string aimHorizontalAxisName = "AimHorizontal";
    [SerializeField] private string aimVerticalAxisName = "AimVertical";

    //******************//
    //    Properties    //
    //******************//

    public Player player { get; private set; }
    public string jumpButton { get { return jumpButtonName; } }
    public string hookButton { get { return hookButtonName; } }
    public string moveHorizontalAxis { get { return moveHorizontalAxisName; } }
    public string aimHorizontalAxis { get { return aimHorizontalAxisName; } }
    public string aimVerticalAxis { get { return aimVerticalAxisName; } }

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        player = ReInput.players.GetPlayer(playerId);
    }
}
