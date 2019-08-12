using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroupFader))]

public class AreaTransitionManager : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float m_transitionTime = 1f;

    //******************//
    //    Properties    //
    //******************//

    public static AreaTransitionManager Instance { get; private set; }

    //**********************//
    //    Private Fields    //
    //**********************//

    private CanvasGroupFader m_fader;
    private bool m_transitioning = false;

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

        m_fader = GetComponent<CanvasGroupFader>();
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private IEnumerator TransitionCoroutine(string _from, SpawnPointData _spawnPoint, GameObject _player, Action _callback = null)
    {
        m_transitioning = true;
        float halfTransitionTime = m_transitionTime / 2f;
        string to = _spawnPoint.area.sceneName;

        if (_player.activeInHierarchy) {
            _player.GetComponent<PlayerMovement>().DisableUserInput(true);
        }

        m_fader.FadeIn(halfTransitionTime);
        yield return new WaitForSeconds(halfTransitionTime);

        AsyncOperation unloadSceneAsync = null;
        if (SceneManager.GetSceneByName(_from).isLoaded) {
            unloadSceneAsync = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(_from));
            while (unloadSceneAsync != null && !unloadSceneAsync.isDone) {
                yield return null;
            }
        }

        _player.SetActive(false);

        AsyncOperation loadSceneAsync = null;
        if (!SceneManager.GetSceneByName(to).isLoaded) {
            loadSceneAsync = SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);
            while (loadSceneAsync != null && !loadSceneAsync.isDone) {
                Debug.LogFormat("Loading game scene: {0}%", loadSceneAsync.progress * 100f);
                yield return null;
            }
        }

        Scene toScene = SceneManager.GetSceneByName(to);
        SceneManager.SetActiveScene(toScene);

        AreaController controller = FindObjectOfType<AreaController>();
        controller.InitializeArea(_player, _spawnPoint);

        _player.SetActive(true);
        _player.GetComponent<PlayerMovement>().DisableUserInput(false);

        m_fader.FadeOut(halfTransitionTime);
        m_transitioning = false;

        _callback?.Invoke();
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void Transition(AreaData _fromArea, SpawnPointData _spawnPoint)
    {
        if (m_transitioning) {
            Debug.LogWarningFormat("{0}: Transition already in progress!", name);
            return;
        }

        if (_fromArea == null) {
            Debug.LogErrorFormat("{0}: Origin area is null!", name);
            return;
        }

        if (_spawnPoint == null) {
            Debug.LogErrorFormat("{0}: Target spawn point is null!", name);
            return;
        }

        Debug.LogFormat("{0}: Transitioning from area {1} to area {2} with spawn point {3}.", name, _fromArea.sceneName, _spawnPoint.area.sceneName, _spawnPoint.name);

        GameObject player = GameManager.Instance.player;
        StartCoroutine(TransitionCoroutine(_fromArea.sceneName, _spawnPoint, player));
    }

    public void LoadGameScene(string _menuScene, SpawnPointData _spawnPoint, Action _callback = null)
    {
        GameObject player = GameManager.Instance.player;
        StartCoroutine(TransitionCoroutine(_menuScene, _spawnPoint, player, _callback));
    }
}
