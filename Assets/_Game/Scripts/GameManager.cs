using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{    
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private string menuSceneName = "";
    [SerializeField] private Area startArea = default;
    [SerializeField] private GameObject player = default;

    //******************//
    //    Properties    //
    //******************//

    public static GameManager Instance
    {
        get;
        private set;
    }

    public GameState state
    {
        get { return m_state; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private GameState m_state;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(this);
        }

        m_state = new GameState(startArea);
    }

    private void Start()
    {
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Additive);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.K)) {
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //}
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private IEnumerator LoadGameCoroutine()
    {
        SceneManager.UnloadSceneAsync(menuSceneName);

        string loadSceneName = m_state.playerState.currentArea.sceneName;

        AsyncOperation loadSceneAsync = null;
        if (!SceneManager.GetSceneByName(loadSceneName).isLoaded) {
            loadSceneAsync = SceneManager.LoadSceneAsync(loadSceneName, LoadSceneMode.Additive);
        }

        while (loadSceneAsync != null && !loadSceneAsync.isDone) {
            Debug.LogFormat("Loading game scene: {0}%", loadSceneAsync.progress * 100f);
            yield return null;
        }

        AreaController controller = FindObjectOfType<AreaController>();
        if (controller) {
            controller.InitializeArea(player, m_state.playerState.currentTransitionId);
        } else {
            Debug.LogErrorFormat("{0}: Could not find AreaController!", name);
        }
        player.SetActive(true);
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void LoadGame()
    {
        StartCoroutine(LoadGameCoroutine());
    }
}
