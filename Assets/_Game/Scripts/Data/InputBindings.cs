using UnityEngine;

[CreateAssetMenu(fileName = "NewInputBindings", menuName = "Pandemic/Input Bindings")]
public class InputBindings : ScriptableObject
{
    public string jumpButtonName = "Jump";
    public string hookButtonName = "Hook";
    public string moveHorizontalAxisName = "MoveHorizontal";
    public string aimHorizontalAxisName = "AimHorizontal";
    public string aimVerticalAxisName = "AimVertical";
}
