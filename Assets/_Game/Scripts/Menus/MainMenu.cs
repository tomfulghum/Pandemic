using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button m_backButton;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    public void SetBackButton(Button _button)
    {
        m_backButton = _button;
    }
    private void Update()
    {
        if (Input.GetButtonDown("Cancel") && m_backButton != null) {
            m_backButton.onClick.Invoke();
        }
    }
    public void PlayGame()
    {
        GameManager.Instance.LoadGame();
    }

    //Exit the game to desktop
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
