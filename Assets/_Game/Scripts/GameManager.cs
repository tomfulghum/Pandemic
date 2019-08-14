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

    [SerializeField] private string m_saveFileName = "";
    [SerializeField] private string m_menuSceneName = "";
    [SerializeField] private GameObject m_ingameUI = default;
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

    public SpawnPointData currentSpawnPoint { get; private set; }

    public GameObject player
    {
        get 
        {
            if (!m_currentPlayer) {
                Debug.LogWarningFormat("{0}: Current player is null!", name);
            }
            return m_currentPlayer;
        }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private GameState m_state = default;
    private BinaryFormatter m_formatter = default;
    private string m_savePath = default;
    private SaveFileData[] m_saveFiles = new SaveFileData[4];
    private int m_currentSaveFileIndex = -1;
    private GameObject m_currentPlayer = default;

    private AreaTransitionManager m_areaTransitionManager = default;

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

        m_formatter = new BinaryFormatter();
        m_savePath = Application.persistentDataPath + "/";
        m_areaTransitionManager = FindObjectOfType<AreaTransitionManager>();
    }

    private void Start()
    {
        RefreshSaveFiles();
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

    private void SavePlayerState(SpawnPointData _spawnPoint)
    {
        m_state.playerState.currentSpawnPoint = _spawnPoint.id;
        m_state.playerState.normalKeyCount = m_currentPlayer.GetComponent<PlayerInventory>().normalKeyCount;
        m_state.playerState.health = m_currentPlayer.GetComponent<PlayerCombat>().currentHealth;
    }

    private void LoadPlayerState()
    {
        m_currentPlayer.GetComponent<PlayerInventory>().normalKeyCount = m_state.playerState.normalKeyCount;
        m_currentPlayer.GetComponent<PlayerCombat>().currentHealth = m_state.playerState.health;
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

    private string GetAreaName(string _spawnPointId)
    {
        SpawnPointData spawnPoint = FindSpawnPoint(_spawnPointId);

        if (spawnPoint == null) {
            Debug.LogErrorFormat("{0}: Could not find spawn point {1}! Maybe the area is missing from the GameManager or the area does not reference the spawn point?", name, _spawnPointId);
            return "";
        }

        if (spawnPoint.area == null) {
            Debug.LogErrorFormat("{0}: Spawn point {1} does not reference an area!", name, _spawnPointId);
            return "";
        }

        return m_areas.Find(x => x.id.Equals(spawnPoint.area.id)).name;
    }

    private SaveFileData LoadSaveFile(string _path)
    {
        SaveFileData data = null;
        if (File.Exists(_path)) {
            FileStream file = File.Open(_path, FileMode.Open);
            GameState gameState = (GameState)m_formatter.Deserialize(file);
            data = new SaveFileData(gameState, GetAreaName(gameState.playerState.currentSpawnPoint));
            file.Close();
        }

        return data;
    }

    private void SaveSaveFile(int _index, GameState _state)
    {
        if (!CheckSaveFileArrayBounds(_index)) {
            return;
        }
        m_saveFiles[_index] = new SaveFileData(_state, GetAreaName(_state.playerState.currentSpawnPoint));
        FileStream file = File.Create(m_savePath + m_saveFileName + _index);
        m_formatter.Serialize(file, _state);

        Debug.LogFormat("{0}: Saved file {1} to {2}.", name, _index, file.Name);

        file.Close();
    }

    private bool CheckSaveFileArrayBounds(int _index)
    {
        if (_index < 0 || _index >= m_saveFiles.Length) {
            Debug.LogErrorFormat("{0}: Save file index out of bounds! Must be larger than 0 and less than {1}.", name, m_saveFiles.Length);
            return false;
        }

        return true;
    }

    private bool SaveFileExists(int _index)
    {
        if (!CheckSaveFileArrayBounds(_index)) {
            return false;
        }

        if (m_saveFiles[_index] == null) {
            Debug.LogErrorFormat("{0}: Save file {1} does not exist!", name, _index);
            return false;
        }

        return true;
    }

    private void RefreshSaveFiles()
    {
        for (int i = 0; i < m_saveFiles.Length; i++) {
            RefreshSaveFile(i);
        }
    }

    private void RefreshSaveFile(int _index)
    {
        if (!CheckSaveFileArrayBounds(_index)) {
            return;
        }

        m_saveFiles[_index] = LoadSaveFile(m_savePath + m_saveFileName + _index);
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void LoadMenuScene()
    {
        m_areaTransitionManager.LoadMenuScene(m_menuSceneName, () => {
            m_ingameUI.SetActive(false);
            Destroy(m_currentPlayer);
            m_currentPlayer = null;
        });
    }

    public void LoadLastSave()
    {
        GameObject oldPlayer = m_currentPlayer;
        m_currentPlayer = Instantiate(m_player);
        currentSpawnPoint = FindSpawnPoint(m_state.playerState.currentSpawnPoint);

        m_areaTransitionManager.LoadGameScene(currentSpawnPoint, () => {
            m_ingameUI.SetActive(true);
            LoadPlayerState();
            Destroy(oldPlayer);
        });
    }

    public void LoadSaveFile(int _index)
    {
        if(!SaveFileExists(_index)) {
            return;
        }

        RefreshSaveFile(_index);

        m_currentSaveFileIndex = _index;
        m_state = m_saveFiles[_index].state;
        LoadLastSave();
    }

    public void CreateSaveFile(int _index)
    {
        if (!CheckSaveFileArrayBounds(_index)) {
            return;
        }

        GameState gameState = new GameState(m_startPoint);
        SaveSaveFile(_index, gameState);
    }

    public void DeleteSaveFile(int _index)
    {
        if (!SaveFileExists(_index)) {
            return;
        }

        File.Delete(m_savePath + m_saveFileName + _index);
        m_saveFiles[_index] = null;
    }

    public void SaveGame(SpawnPointData _spawnPoint)
    {
        currentSpawnPoint = _spawnPoint;
        SavePlayerState(_spawnPoint);

        SaveSaveFile(m_currentSaveFileIndex, m_state);
    }

    public SaveFileData GetSaveFileData(int _index)
    {
        RefreshSaveFile(_index);
        return m_saveFiles[_index];
    }
}
