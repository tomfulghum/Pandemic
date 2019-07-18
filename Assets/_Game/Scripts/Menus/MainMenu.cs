using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    public void PlayGame()
    {
        MenuManager.Instance.LoadGame();
    }

    //Exit the game to desktop
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
