using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MenuButton : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] int thisIndex;
    [SerializeField] Color unselectedColor;
    [SerializeField] Color selectedColor;

    //*******************//
    //   Public Fields   //
    //*******************//

    public TextMeshProUGUI textMeshPro;
    
    //********************//
    //   Private Fields   //
    //********************//

    private ButtonIndex thisMenu;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        thisMenu = transform.parent.GetComponentInParent<ButtonIndex>();
    }
    void Update()
    {
        if (thisMenu.index == thisIndex) {
            textMeshPro.color = selectedColor;
            if (Input.GetButtonDown("Submit")) {
                transform.parent.GetComponent<Button>().onClick.Invoke();
            }
        } else {
            textMeshPro.color = unselectedColor;
        }
    }
}
