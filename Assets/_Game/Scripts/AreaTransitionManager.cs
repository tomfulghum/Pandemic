using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class AreaTransitionManager : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private GameObject playerFollowCamera = default;

    //******************//
    //    Properties    //
    //******************//

    public static AreaTransitionManager Instance { get; private set; }

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
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private IEnumerator TransitionCoroutine(string _from, string _to, int _transitionId, GameObject _player)
    {
        AsyncOperation loadSceneAsync = null;
        if (!SceneManager.GetSceneByName(_to).isLoaded) {
            loadSceneAsync = SceneManager.LoadSceneAsync(_to, LoadSceneMode.Additive);
        }

        while (loadSceneAsync != null && !loadSceneAsync.isDone) {
            yield return null;
        }

        Scene to = SceneManager.GetSceneByName(_to);
        SceneManager.SetActiveScene(to);
        //SceneManager.MoveGameObjectToScene(_player, to);

        AsyncOperation unloadSceneAsync = null;
        if (SceneManager.GetSceneByName(_from).isLoaded) {
            unloadSceneAsync = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(_from));
        }

        while (unloadSceneAsync != null && !unloadSceneAsync.isDone) {
            yield return null;
        }

        AreaController controller = FindObjectOfType<AreaController>();
        AreaTransition transition = controller.GetTransition(_transitionId);
        _player.transform.position = transition.spawnPoint.position;
        CinemachineVirtualCamera cam = Instantiate(playerFollowCamera, _player.transform.position, Quaternion.identity).GetComponent<CinemachineVirtualCamera>();
        cam.Follow = _player.transform;
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
