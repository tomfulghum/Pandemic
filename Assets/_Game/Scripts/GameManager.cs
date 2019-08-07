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
    [SerializeField] private SpawnPointData m_startPoint = default;
    [SerializeField] private GameObject m_player = default;
    [SerializeField] private List<AreaData> m_areas = default;

    //******************//
    //    Properties    //
    //******************//

    public static GameManager Instance { get; private set; }

    public GameState state
    {
        get { return m_state; }
    }

    public SpawnPointData currentSpawnPoint { get; set; }

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

        m_state = new GameState(m_startPoint);
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

        if (currentSpawnPoint == null) {
            Debug.LogFormat("{0}: Resetting spawn point.", name);
            currentSpawnPoint = m_startPoint;
        }

        string loadSceneName = currentSpawnPoint.area.sceneName;

        AsyncOperation loadSceneAsync = null;
        if (!SceneManager.GetSceneByName(loadSceneName).isLoaded) {
            loadSceneAsync = SceneManager.LoadSceneAsync(loadSceneName, LoadSceneMode.Additive);
        }

        while (loadSceneAsync != null && !loadSceneAsync.isDone) {
            Debug.LogFormat("{0}: Loading game scene: {1}%", name, loadSceneAsync.progress * 100f);
            yield return null;
        }

        AreaController controller = FindObjectOfType<AreaController>();
        if (controller) {
            controller.InitializeArea(m_player, currentSpawnPoint);
        } else {
            Debug.LogErrorFormat("{0}: Could not find AreaController!", name);
        }
        m_player.SetActive(true);
    }

    private void SavePlayerState()
    {
        m_state.playerState.currentSpawnPoint = currentSpawnPoint.id;
        m_state.playerState.normalKeyCount = m_player.GetComponent<PlayerInventory>().normalKeyCount;
    }

    private void LoadPlayerState()
    {
        currentSpawnPoint = FindSpawnPoint(m_state.playerState.currentSpawnPoint);
        m_player.GetComponent<PlayerInventory>().AddNormalKeys(m_state.playerState.normalKeyCount);
    }

    private SpawnPointData FindSpawnPoint(string _id)
    {
        SpawnPointData spawnPoint = null;
        foreach (var area in m_areas) {
            spawnPoint = area.FindSpawnPoint(_id);

            if (spawnPoint) {
                break;
            }
        }

        return spawnPoint;
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

            LoadPlayerState();
        }

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
