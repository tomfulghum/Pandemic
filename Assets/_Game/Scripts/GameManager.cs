using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{    
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private string m_menuSceneName = "";
    [SerializeField] private Area m_startArea = default;
    [SerializeField] private GameObject m_player = default;
    [SerializeField] private List<Area> m_areas = default;

    //******************//
    //    Properties    //
    //******************//

    public static GameManager Instance { get; private set; }

    public GameState state
    {
        get { return m_state; }
    }

    public Area currentArea { get; set; }

    //**********************//
    //    Private Fields    //
    //**********************//

    private GameState m_state = default;
    private BinaryFormatter m_formatter = default;
    private string m_savePath = default;

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

        m_state = new GameState(m_startArea);
        m_formatter = new BinaryFormatter();
        m_savePath = Application.persistentDataPath + "/savefile.sav";
    }

    private void Start()
    {
        SceneManager.LoadScene(m_menuSceneName, LoadSceneMode.Additive);
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
        SceneManager.UnloadSceneAsync(m_menuSceneName);

        string loadSceneName = m_areas.Find(x => x.id == m_state.playerState.currentArea).sceneName;

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
            controller.InitializeArea(m_player, m_state.playerState.currentTransitionId);
            currentArea = controller.area;
        } else {
            Debug.LogErrorFormat("{0}: Could not find AreaController!", name);
        }
        m_player.SetActive(true);
    }

    private void SavePlayerState()
    {
        PlayerState playerState = m_state.playerState;
        playerState.currentArea = currentArea.id;
        playerState.currentTransitionId = 0;
        playerState.normalKeyCount = m_player.GetComponent<PlayerInventory>().normalKeyCount;
    }

    private void LoadPlayerState()
    {
        m_player.GetComponent<PlayerInventory>().AddNormalKeys(m_state.playerState.normalKeyCount);
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void LoadGame()
    {
        if (File.Exists(m_savePath)) {
            FileStream file = File.Open(m_savePath, FileMode.Open);
            m_state = (GameState)m_formatter.Deserialize(file);
            file.Close();
        }

        LoadPlayerState();
        StartCoroutine(LoadGameCoroutine());
    }

    public void SaveGame()
    {
        SavePlayerState();

        FileStream file = File.Create(m_savePath);
        Debug.LogFormat("Saving game state to: {0}", file.Name);
        m_formatter.Serialize(file, m_state);
        file.Close();
    }
}
