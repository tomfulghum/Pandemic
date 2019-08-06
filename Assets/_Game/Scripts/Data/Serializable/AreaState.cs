using System.Collections.Generic;

[System.Serializable]
public class AreaState 
{
    public Area area;
    public List<NormalKeyState> normalKeyStates;
    public List<LeverState> leverStates;

    public AreaState(Area _area, List<NormalKey> _normalKeys, List<Lever> _levers)
    {
        area = _area;

        normalKeyStates = new List<NormalKeyState>();
        foreach (var normalKey in _normalKeys) {
            normalKeyStates.Add(new NormalKeyState());
        }

        leverStates = new List<LeverState>();
        foreach (var lever in _levers) {
            leverStates.Add(new LeverState());
        }
    }
}
