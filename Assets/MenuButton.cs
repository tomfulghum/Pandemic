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

    [SerializeField] private ButtonHighlightData m_buttonHighlight;

    //********************//
    //   Private Fields   //
    //********************//

    private List<TextMeshProUGUI> textMeshProList;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        textMeshProList = new List<TextMeshProUGUI>(GetComponentsInChildren<TextMeshProUGUI>(true));
    }
    void Update()
    {
        if (gameObject == EventSystem.current.currentSelectedGameObject) {
            foreach (var textMP in textMeshProList) {
                textMP.color = m_buttonHighlight.selectedColor;
            }
        } else {
            foreach (var textMP in textMeshProList) {
                textMP.color = m_buttonHighlight.unselectedColor;
            }
        }
    }
}