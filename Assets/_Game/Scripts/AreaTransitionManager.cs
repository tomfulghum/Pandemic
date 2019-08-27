using System;
using System.Collections;
using System.Collections.Generic;
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

    //*********************//
    //    Public Fields    //
    //*********************//

    public delegate void OnAreaLoadedDelegate();
    public static OnAreaLoadedDelegate onAreaLoaded;

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

    private IEnumerator TransitionCoroutine(string _sceneName, GameObject _player, SpawnPointData _spawnPoint = null, Action _callback = null)
    {
        m_transitioning = true;
        float halfTransitionTime = m_transitionTime / 2f;

        if (_player.activeInHierarchy) {
            _player.GetComponent<PlayerMovement>().DisableUserInput(true);
        }

        m_fader.FadeIn(halfTransitionTime);
        yield return new WaitForSeconds(halfTransitionTime);

        List<Scene> loadedScenes = new List<Scene>();
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            loadedScenes.Add(SceneManager.GetSceneAt(i));
        }

        foreach (Scene scene in loadedScenes) {
            AsyncOperation unloadSceneAsync = null;
            if (scene.buildIndex != 0 && scene.isLoaded) {
                unloadSceneAsync = SceneManager.UnloadSceneAsync(scene);
                while (unloadSceneAsync != null && !unloadSceneAsync.isDone) {
                    yield return null;
                }
            }
        }

        _player.SetActive(false);

        AsyncOperation loadSceneAsync = null;
        if (!SceneManager.GetSceneByName(_sceneName).isLoaded) {
            loadSceneAsync = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            while (loadSceneAsync != null && !loadSceneAsync.isDone) {
                Debug.LogFormat("{0}: Loading scene: {1}%", name, loadSceneAsync.progress * 100f);
                yield return null;
            }
        }

        Scene toScene = SceneManager.GetSceneByName(_sceneName);
        SceneManager.SetActiveScene(toScene);

        if (_spawnPoint != null) {

            AreaController controller = FindObjectOfType<AreaController>();
            controller.InitializeArea(_player, _spawnPoint);

            _player.SetActive(true);
            _player.GetComponent<PlayerMovement>().DisableUserInput(false);
        }

        m_fader.FadeOut(halfTransitionTime);
        m_transitioning = false;

        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));

        _callback?.Invoke();
        onAreaLoaded?.Invoke();
    }

    private IEnumerator SetbackCoroutine(GameObject _player, Vector2 _position)
    {
        m_transitioning = true;
        float halfTransitionTime = m_transitionTime / 2f;

        _player.GetComponent<PlayerMovement>().DisableUserInput(true);

        m_fader.FadeIn(halfTransitionTime);
        yield return new WaitForSeconds(halfTransitionTime);

        _player.SetActive(false);
        _player.transform.position = _position;
        _player.SetActive(true);

        _player.GetComponent<PlayerMovement>().DisableUserInput(false);

        m_fader.FadeOut(halfTransitionTime);
        m_transitioning = false;
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

        if (_spawnPoint.area == null) {
            Debug.LogErrorFormat("{0}: Target spawn point's area is null!", name);
            return;
        }

        Debug.LogFormat("{0}: Transitioning from area {1} to area {2} with spawn point {3}.", name, _fromArea.sceneName, _spawnPoint.area.sceneName, _spawnPoint.name);

        GameObject player = GameManager.Instance.player;
        StartCoroutine(TransitionCoroutine(_spawnPoint.area.sceneName, player, _spawnPoint));
    }

    public void LoadGameScene(SpawnPointData _spawnPoint, Action _callback = null)
    {
        if (m_transitioning) {
            Debug.LogWarningFormat("{0}: Transition already in progress!", name);
            return;
        }

        Debug.LogFormat("{0}: Loading area {1} with spawn point {2}.", name, _spawnPoint.area.sceneName, _spawnPoint.name);

        GameObject player = GameManager.Instance.player;
        StartCoroutine(TransitionCoroutine(_spawnPoint.area.sceneName, player, _spawnPoint, _callback));
    }

    public void LoadMenuScene(string _menuScene, Action _callback = null)
    {
        if (m_transitioning) {
            Debug.LogWarningFormat("{0}: Transition already in progress!", name);
            return;
        }

        Debug.LogFormat("{0}: Loading menu scene.", name);

        GameObject player = GameManager.Instance.player;
        StartCoroutine(TransitionCoroutine(_menuScene, player, null, _callback));
    }

    public void Setback(GameObject _player, Vector2 _position)
    {
        if (m_transitioning) {
            Debug.LogWarningFormat("{0}: Transition already in progress!", name);
            return;
        }

        StartCoroutine(SetbackCoroutine(_player, _position));
    }
}
