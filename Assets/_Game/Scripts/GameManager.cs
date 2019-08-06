using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //[SerializeField] private List<Area> m_areas = default;

    public static GameManager Instance
    {
        get;
        private set;
    }

    public GameState state
    {
        get { return m_state; }
    }

    private GameState m_state;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(this);
        }

        m_state = new GameState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
