using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroupFader))]

public class AreaTransitionManager : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float transitionTime = 1f;

    //******************//
    //    Properties    //
    //******************//

    public static AreaTransitionManager Instance { get; private set; }

    //**********************//
    //    Private Fields    //
    //**********************//

    private CanvasGroupFader fader;

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

        fader = GetComponent<CanvasGroupFader>();
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private IEnumerator TransitionCoroutine(string _from, string _to, int _transitionId, GameObject _player)
    {
        float halfTransitionTime = transitionTime / 2f;

        _player.GetComponent<PlayerMovement>().DisableUserInput(true);
        fader.FadeIn(halfTransitionTime);
        yield return new WaitForSeconds(halfTransitionTime);

        AsyncOperation loadSceneAsync = null;
        if (!SceneManager.GetSceneByName(_to).isLoaded) {
            loadSceneAsync = SceneManager.LoadSceneAsync(_to, LoadSceneMode.Additive);
        }

        while (loadSceneAsync != null && !loadSceneAsync.isDone) {
            yield return null;
        }

        _player.GetComponent<PlayerMovement>().DisableUserInput(false);

        Scene to = SceneManager.GetSceneByName(_to);
        SceneManager.SetActiveScene(to);

        AsyncOperation unloadSceneAsync = null;
        if (SceneManager.GetSceneByName(_from).isLoaded) {
            unloadSceneAsync = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(_from));
        }

        while (unloadSceneAsync != null && !unloadSceneAsync.isDone) {
            yield return null;
        }

        AreaController controller = FindObjectOfType<AreaController>();
        controller.InitializeArea(_player, _transitionId);

        fader.FadeOut(halfTransitionTime);
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void Transition(Area _fromArea, Area _toArea, int _transitionId)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(TransitionCoroutine(_fromArea.sceneName, _toArea.sceneName, _transitionId, player));
    }
}
