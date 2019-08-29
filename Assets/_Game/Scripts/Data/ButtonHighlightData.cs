using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewButtonHighlightColor", menuName = "Pandemic/Button Highlight Data")]
public class ButtonHighlightData : ScriptableObject
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    public Color unselectedColor = default;
    public Color selectedColor = default;
   
}
