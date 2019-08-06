using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class AreaController : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private Area m_area = default;
    [SerializeField] private GameObject m_playerFollowCamera = default;
    [SerializeField] private List<AreaTransition> m_areaTransitions = default;

    [Header("Saveable Objects")]

    [SerializeField] private List<NormalKey> m_normalKeys = default;
    [SerializeField] private List<Lever> m_levers = default;
    [SerializeField] private List<Enemy> m_enemies = default;

    //******************//
    //    Properties    //
    //******************//

    public Area area { get { return m_area; } }

    //**********************//
    //    Private Fields    //
    //**********************//

    private AreaState m_state = default;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_state = new AreaState(m_area.id, m_normalKeys, m_levers);
    }

    private void OnValidate()
    {
        RegisterWithTransitions();
    }

    private void Start()
    {
        RegisterWithTransitions();
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void RegisterWithTransitions()
    {
        foreach (var transition in m_areaTransitions) {
            if (transition) {
                transition.controller = this;
            }
        }
    }

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

    public void InitializeArea(GameObject _player, int _transitionId)
    {
        AreaTransition transition = m_areaTransitions.Find(x => x.transitionId == _transitionId);
        if (transition) {
            GameState gameState = GameManager.Instance.state;
            AreaState areaState = gameState.GetAreaState(m_area);
            if (areaState == null) {
                gameState.areaStates.Add(m_state);
            } else {
                m_state = areaState;
            }

            InitializeNormalKeys();
            InitializeLevers();

            _player.transform.position = transition.spawnPoint.position;
            CinemachineVirtualCamera cam = Instantiate(m_playerFollowCamera, _player.transform.position, Quaternion.identity).GetComponent<CinemachineVirtualCamera>();
            cam.Follow = _player.transform;
        } else {
            Debug.LogError("[AreaController] Invalid transition ID! (" + _transitionId + ")");
        }
    }
}
