using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
public class MenuButton : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private Color unselectedColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    
    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
    }
    void Update()
    {
        if (gameObject == EventSystem.current.currentSelectedGameObject) {
            textMeshPro.color = selectedColor;
            //if (Input.GetButtonDown("Submit")) {
            //    GetComponent<Button>().onClick.Invoke();
            //}
        } else {
            textMeshPro.color = unselectedColor;
        }
    }
}