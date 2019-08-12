using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class AreaController : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private AreaData m_area = default;
    [SerializeField] private GameObject m_playerFollowCamera = default;
    [SerializeField] private PolygonCollider2D m_areaCameraBounds = default;

    [Header("Saveable Objects")]

    [SerializeField] private List<NormalKey> m_normalKeys = default;
    [SerializeField] private List<Lever> m_levers = default;
    [SerializeField] private List<Enemy> m_enemies = default;

    //******************//
    //    Properties    //
    //******************//

    public AreaData area { get { return m_area; } }

    //**********************//
    //    Private Fields    //
    //**********************//

    private AreaState m_state = default;
    private List<SpawnPoint> m_spawnPoints = default;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_state = new AreaState(m_area.id, m_normalKeys, m_levers);
        m_spawnPoints = new List<SpawnPoint>(FindObjectsOfType<SpawnPoint>());
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void InitializeNormalKeys()
    {
        for (int i = 0; i < m_state.normalKeyStates.Count; i++) {
            m_normalKeys[i].SetState(m_state.normalKeyStates[i]);
        }
    }

    private void InitializeLevers()
    {
        for (int i = 0; i < m_state.leverStates.Count; i++) {
            m_levers[i].SetState(m_state.leverStates[i]);
        }
    }

    private void InitializeEnemies()
    {
        
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void InitializeArea(GameObject _player, SpawnPointData _spawnPoint)
    {
        SpawnPoint spawnPoint = m_spawnPoints.Find(x => x.spawnPointData == _spawnPoint);
        if (spawnPoint) {
            GameState gameState = GameManager.Instance.state;
            AreaState areaState = gameState.GetAreaState(m_area);
            if (areaState == null) {
                gameState.areaStates.Add(m_state);
            } else {
                m_state = areaState;
            }

            InitializeNormalKeys();
            InitializeLevers();

            _player.transform.position = spawnPoint.transform.position;
            CinemachineVirtualCamera cam = Instantiate(m_playerFollowCamera, _player.transform.position, Quaternion.identity).GetComponent<CinemachineVirtualCamera>();
            cam.Follow = _player.transform;
            cam.GetComponent<CinemachineConfiner>().m_BoundingShape2D = m_areaCameraBounds;
        } else {
            Debug.LogErrorFormat("{0}: Could not find spawn point {1}!", name, _spawnPoint.name);
        }
    }

    public SpawnPointData GetSpawnPointData(string _id)
    {
        return m_spawnPoints.Find(x => x.spawnPointData.id.Equals(_id)).spawnPointData;
    }
}
