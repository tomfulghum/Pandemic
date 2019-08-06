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

    private IEnumerator TransitionCoroutine(string _from, string _to, int _transitionId, GameObject _player)
    {
        m_transitioning = true;
        float halfTransitionTime = m_transitionTime / 2f;

        _player.GetComponent<PlayerMovement>().DisableUserInput(true);
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
        if (!SceneManager.GetSceneByName(_to).isLoaded) {
            loadSceneAsync = SceneManager.LoadSceneAsync(_to, LoadSceneMode.Additive);
            while (loadSceneAsync != null && !loadSceneAsync.isDone) {
                Debug.LogFormat("Loading game scene: {0}%", loadSceneAsync.progress * 100);
                yield return null;
            }
        }

        Scene to = SceneManager.GetSceneByName(_to);
        SceneManager.SetActiveScene(to);

        AreaController controller = FindObjectOfType<AreaController>();
        controller.InitializeArea(_player, _transitionId);
        _player.SetActive(true);
        _player.GetComponent<PlayerMovement>().DisableUserInput(false);

        m_fader.FadeOut(halfTransitionTime);
        m_transitioning = false;
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void Transition(Area _fromArea, Area _toArea, int _transitionId)
    {
        if (m_transitioning) {
            Debug.LogWarningFormat("{0}: Transition already in progress!", name);
            return;
        }

        Debug.LogFormat("{0}: Transitioning from area {1} to area {2} with transition id {3}.", name, _fromArea.sceneName, _toArea.sceneName, _transitionId);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(TransitionCoroutine(_fromArea.sceneName, _toArea.sceneName, _transitionId, player));
    }
}
