using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonIndex : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] bool keyDown = false;
    [SerializeField] int maxIndex = 0;
    [SerializeField] Button backButton = default;

    //*******************//
    //   Public Fields   //
    //*******************//

    public int index;
    public AudioSource audioSource;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Update()
    {
        //Set Menu Button Index
        if (Input.GetAxis("Vertical") != 0) {
            if (!keyDown) {
                if (Input.GetAxis("Vertical") < 0) {
                    if (index < maxIndex) {
                        index++;
                    } else {
                        index = 0;
                    }
                } else if (Input.GetAxis("Vertical") > 0) {
                    if (index > 0) {
                        index--;
                    } else {
                        index = maxIndex;
                    }
                }
                keyDown = true;
            }
        } else {
            keyDown = false;
        }
        if (Input.GetButtonDown("Cancel") && backButton != null) {
            backButton.onClick.Invoke();
        }
    }
}