using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] public GameObject PauseMenuUI;

    public static bool GameIsPaused = false;

    void Update()
    {
        if (Input.GetButtonDown("Pause")) {
            if (GameIsPaused) {
                Resume();
            } else {
                Pause();
            }
        }
    }

    public void Pause()
    {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void Resume()
    {
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void LoadMainMenu()
    {
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameManager.Instance.LoadMenuScene();
    }
}
